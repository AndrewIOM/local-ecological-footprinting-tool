import sys, os.path, json, time, argparse, ConfigParser, getpass, peewee
import requests, progress, wget, zipfile
import gdal, csv
from gdalconst import *
from osgeo import osr
from progress.spinner import Spinner
from datetime import datetime
from requests.auth import HTTPBasicAuth
from peewee import *
from subprocess import Popen, PIPE, call

prompt = False

print "\n-- GBIF MIRROR UPDATE TOOL --\n"

# ==============================================================================
# utility functions

def stage(label):
        print "\n" + str(datetime.now().strftime("%Y-%m-%d %H:%M")) + " | " + label

# asks the user a y/n question - http://stackoverflow.com/a/3041990
def ask(question, default="yes"):
        print "\n"
        valid = {"yes": True, "y": True, "ye": True,
                         "no": False, "n": False}
        if default is None:
                q = " [y/n] "
        elif default == "yes":
                q = " [Y/n] "
        elif default == "no":
                q = " [y/N] "
        else:
                raise ValueError("invalid default answer: '%s'" % default)

        while True:
                sys.stdout.write(question + q)
                choice = raw_input().lower()
                if default is not None and choice == '':
                        return valid[default]
                elif choice in valid:
                        return valid[choice]
                else:
                        sys.stdout.write("Please respond with 'yes' or 'no' "
                                                         "(or 'y' or 'n').\n")

# asks the user a y/n question but defaults to yes if prompt is false
def ask_stage(question, default="yes"):
        stage = True
        if prompt:
                stage = ask(question, default)
        return stage

# prompts the user to enter an string value, which accepts a default option
def get_string(prompt, default=""):
        print "\n"
        in_string = raw_input(prompt + ' [' + str(default) + ']: ')
        out_string = None
        if in_string != "":
                out_string = str(in_string)
        else:
                print "Defaulting to '" + str(default) + "'"
                out_string = str(default)
        return out_string

# ==============================================================================
# read from the config files
config = ConfigParser.ConfigParser()
config.read("config.cfg")
database_name = config.get("Connection", "database_name")
database_host = config.get("Connection", "database_host")
database_port = int(config.get("Connection", "database_port"))
database_user = config.get("Connection", "database_user")
database_password = config.get("Connection", "database_password")
gbif_table = config.get("Connection", "gbif_table")
gbif_org_lookup_table = config.get("Connection", "gbif_org_lookup_table")
gbif_coordinate_table = config.get("Connection", "gbif_coordinate_table")
column_names = json.loads(config.get("Useful Fields", "column_names"))

taxonconfig = ConfigParser.ConfigParser()
taxonconfig.read("taxonkeys.cfg")

# setup parser
parser = argparse.ArgumentParser(description="Downloads new and updated \
        occurrence data from GBIF using the GBIF API and appends it to a local \
        SQL database", epilog="Example: $ python update.py 2016-01-01 2016-01-31 \
        -p")
parser.add_argument("start_date", metavar="START", type=str, nargs="+",
                                        help="The start date (YYYY-MM-DD)")
parser.add_argument("end_date", metavar="END", type=str, nargs="+",
                                        help="The end date (YYYY-MM-DD)")
parser.add_argument("-p", action="store_true", dest="prompt", help="Optional \
        flag to prompt at each step")

# parse command line arguments
args = parser.parse_args()
start_date = str(args.start_date[0])
end_date = str(args.end_date[0])
prompt = args.prompt

# ==============================================================================
# connect to mysql
db = MySQLDatabase(database_name, user=database_user, host=database_host,
        port=database_port, passwd=database_password, local_infile=1)
db.connect()

print "Successfully connected to the MySQL database as '" + database_user + "'"

# ==============================================================================
# which taxon group?
print "\nAvailable taxon groups:"
for i in range(len(taxonconfig.sections())):
        print "{label} {group}".format(label=i, group=taxonconfig.sections()[i])
iselect=0
if(prompt):
        while True:
                choice = get_string("Enter the taxon group", "0")
                if choice.isdigit() and int(choice)>=0 and int(choice)<len(taxonconfig.sections()):
                        break
        iselect = int(choice)
taxongroup = taxonconfig.sections()[iselect]

# ==============================================================================
# query GBIF for a download key

download_key = ""
if ask_stage("Get new GBIF download key?"):
        taxonkeys = taxonconfig.get(taxongroup, "taxon_keys")

        # setup predicate
        predicate = ""

        # a dictionary of "{{ }}" surrounded strings in the json file to replace
        # with python variables
        predicate_templates = {
                "start_date": start_date,
                "end_date": end_date,
                "keys": taxonkeys
        }

        with open ("predicate.json", "r") as predicate_file:
                predicate = predicate_file.read()

        for i in xrange(0, len(predicate_templates)):
                predicate = predicate.replace("{{" + str(predicate_templates.keys()[i])
                        + "}}", str(predicate_templates.values()[i]))
        print predicate

        post_request = requests.post(
                "http://api.gbif.org/v1/occurrence/download/request",
                data=predicate, auth=HTTPBasicAuth('oxfordleft', 'Oxf*rdL3FT'),
                headers = {'content-type': 'application/json'})

        download_key = post_request.text
        print "Received download key: " + download_key + "\n"

if download_key is "":
        download_key = get_string("Please enter the download key")
        print "\n"

# ==============================================================================
# poll the GBIF server for a download url from the download key

file_url = ""
total_seconds = 0
start_time = str(datetime.now().strftime("%Y-%m-%d %H:%M"))
print "Download not yet ready - started at " + start_time
spinner = Spinner("\tWaiting for download to be ready ")

while True:
        ready = False
        if total_seconds % 30 == 0:
                get_request = requests.head(
                        "http://api.gbif.org/v1/occurrence/download/request/" + download_key
                )
                content_type = get_request.headers["content-type"]
                url = get_request.url
                ready = (content_type.split(";")[0] == "application/octet-stream")
        if ready:
                # has returned zip file
                file_url = url + ".zip"
                print "\n"
                print "Download ready at: " + file_url
                break
        else:
                spinner.next()
                total_seconds += 0.25
                time.sleep(0.25)

# ==============================================================================
# download the zip file from the provided url
zip_file = ""
if ask_stage("Download the zip file?"):
        stage("Downloading zip file")
        if not os.path.exists('download'):
                os.makedirs('download')
        zip_file = wget.download(url,'download')

if zip_file == "":
        zip_file = get_string("Please enter the location of the zip file",
                "download/"+download_key + ".zip")

# ==============================================================================
# unzip the zip file

dest_dir = zip_file[:-4].split('/')[-1]
if ask_stage("Unzip the zip file?"):
        stage("Unzipping " + zip_file)
        with zipfile.ZipFile(zip_file) as zf:
                for member in zf.infolist():
                        # path traversal defense copied from
                        # http://hg.python.org/cpython/file/tip/Lib/http/server.py#l789
                        words = member.filename.split('/')
                        path = "download/"+dest_dir
                        for word in words[:-1]:
                                drive, word = os.path.splitdrive(word)
                                head, word = os.path.split(word)
                                if word in (os.curdir, os.pardir, ''): continue
                                path = os.path.join(path, word)
                        zf.extract(member, path)
else:
        dest_dir = get_string("Please enter the name of the extracted folder",
                download_key)
# ==============================================================================
# strip corrupt, non-species and land-based rows from the downloaded csv

if ask_stage("Strip any corrupt/non-species/land-based rows from the csv?"):
        stage("Stripping corrupt/non-species/land-based rows from " + dest_dir + "/" + dest_dir + ".csv")

        # check if the download was successful
        if not os.path.isfile("download/"+dest_dir + "/" + dest_dir + ".csv"):
                print "ERROR: no data downloaded - there may be no data for the range"
                sys.exit()

        dataset = gdal.Open("land30sec_global.tif", GA_ReadOnly)
        geotransform = dataset.GetGeoTransform()
        srs = osr.SpatialReference()
        srs.ImportFromWkt(dataset.GetProjection())
        srsLatLong = srs.CloneGeogCS()
        ct = osr.CoordinateTransformation(srsLatLong, srs)
        srcband = dataset.GetRasterBand(1)

        width = dataset.RasterXSize
        height = dataset.RasterYSize

        minx = geotransform[0]
        miny = geotransform[3] + height*geotransform[5]
        maxx = geotransform[0] + width*geotransform[1]
        maxy = geotransform[3]

        def coord2pixel(lat, lon):
                x = int((lon - geotransform[0]) / geotransform[1])
                y = int((lat - geotransform[3]) / geotransform[5])
                return (x, y)

        def get_value(lat, lon):
                if lat < miny or lat > maxy or lon < minx or lon > maxx:
                                return None

                pixel = coord2pixel(lat, lon)
                try:
                        return srcband.ReadAsArray(pixel[0], pixel[1], 1, 1)[0][0]
                except Exception:
                        print "Record out of range: (lat, lon) = (" + str(lat) + ", " +  str(lon) + ")"
                        return None

        outfile = "download/"+dest_dir + "/" + dest_dir + "_fix.csv"
        with open(outfile, 'ab') as outcsvfile:
                with open("download/"+dest_dir + "/" + dest_dir + ".csv") as incsvfile:
                        first = True
                        for line in incsvfile:
                                if(first):
                                        first = False
                                        continue

                                try:
                                        rows = csv.reader([line], delimiter="\t")
                                        row = next(rows)
                                except Exception:
                                        continue
                                if(len(row) != 50):
                                        print str(row) + " is corrupt"
                                        continue

                                lat = 0.0
                                lon = 0.0

                                if(row[21] == "" or row[22] == ""):
                                        print str(row) + " is corrupt - lat/long empty"
                                        continue

                                try:
                                        lat = float(row[21])
                                        lon = float(row[22])
                                except ValueError:
                                        print str(row) + " is corrupt - lat/long corrupt"
                                        continue

                                if get_value(lat, lon) != 0:
                                        # ocean record
                                        continue

                                taxonRank = row[11]
                                if taxonRank != "SPECIES":
                                        # non-species
                                        continue

                                # no errors
                                outcsvfile.write(line)

# ==============================================================================
# process the data

if ask_stage("Process the downloaded data?"):
        stage("Processing data")

        # check if the download was successful
        if not os.path.isfile("download/"+dest_dir+"/"+dest_dir + ".csv"):
                print "ERROR: no data downloaded - there may be no data for the range"
                sys.exit()

        useful_fields = dict((k, 2) for k in column_names)

        # creates a column_info string, that is used in the sql query
        # to specify which columns should be ignored
        columns = 0
        print "Opening CSV"
        with open("download/"+dest_dir+"/"+dest_dir + ".csv") as f:
                for line in f:
                        print line
                        line = line.split("\t")
                        columns = len(line)
                        for i in xrange(0, len(line)):
                                if ("gbif_" + line[i]) in useful_fields:
                                        useful_fields["gbif_" + line[i]] = i
                        break
        s = ""
        for i in xrange(0, columns):
                if i in useful_fields.values():
                        for k, v in useful_fields.iteritems():
                                if v == i:
                                        s += "@var" + str(v) + ","
                else:
                        s += "@dummy,"
        column_info = "(" + s[:-1] + ")"

        s = ""
        latno = 0
        lonno = 0
        for k, v in useful_fields.iteritems():
                if k == "gbif_decimalLatitude":
                        latno = v
                elif k == "gbif_decimalLongitude":
                        lonno = v
                s += (str(k) + " = @var" + str(v) + ",")

        # set taxonomic group
        s += "taxonomicgroup = '{group}'".format(group=taxongroup)

        set_info = "SET " + s

        # To do a LOAD DATA, coordinate must be null, which means it can't be indexed, which means it can't be part of the master table
        # Therefore place coordinate into a coordinates table

        sql = ("LOAD DATA LOCAL INFILE '" + os.path.abspath(
                "download/"+dest_dir+"/"+dest_dir + "_fix.csv").replace("\\", "\\\\") + "' "
                        "REPLACE INTO TABLE `{table_name}` "
                        "FIELDS TERMINATED BY '\\t' "
                        "LINES TERMINATED BY '\\n' "
                "" + column_info + " " + set_info + ";").format(table_name=gbif_table)
        print(sql)
        db.execute_sql(sql)

        # Now add the new coordinates
        sql = ("INSERT INTO `{coord_table_name}`(gbif_gbifid,coordinate) "
                "SELECT gbif_gbifid, POINT(gbif_decimallatitude, gbif_decimallongitude) "
                "FROM `{master_table_name}` "
                "WHERE gbif_gbifid NOT IN (SELECT gbif_gbifid FROM `{coord_table_name}`)").format(coord_table_name=gbif_coordinate_table, master_table_name=gbif_table)
        print(sql)
        db.execute_sql(sql)

stage("Finished processing data")

# ==============================================================================
# update the lookup table of orgkeys

if ask_stage("Update the organisation key lookup table?"):
        stage("Updating orgkey lookup table")

        sql_response = db.execute_sql(
                "SELECT DISTINCT `gbif_publishingorgkey` FROM `" + gbif_table + "` ;"
        )

        all_keys = sql_response.fetchall()

        existing_keys_sql = db.execute_sql(
                "SELECT `gbif_publishingorgkey` FROM `" + gbif_org_lookup_table + "` ;"
        )

        existing_keys = existing_keys_sql.fetchall()

        new_keys = list(set(all_keys) - set(existing_keys))

        with open("download/"+dest_dir+"/new_keys.csv", "w") as f:
                for s in new_keys:
                        f.write(str(s[0]) + "\n")

        for key in new_keys:
                # use the gbif api to get the title of the organisation
                org_request = requests.get(
                        "http://api.gbif.org/v1/organization/" + str(key[0])
                )
                try:
                        orgtitle = (org_request.json()["title"]).encode('utf-8').strip()
                except Exception, e:
                        print key[0]
                        continue

                sql = ("INSERT INTO `" + gbif_org_lookup_table + "` "
                        "(gbif_publishingorgkey, title) VALUES (\"" + str(key[0]) + "\", "
                        "\"" + orgtitle + "\");"
                )
                try:
                        db.execute_sql(sql)
                except Exception, e:
                        print key[0]
                        continue

stage("Process Complete!")
