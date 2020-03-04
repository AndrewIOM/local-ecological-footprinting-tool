LEFT: Local Ecological Footprinting Tool
===

LEFT and Marine LEFT are .NET Core web applications by the Long-Term Ecology Lab, University of Oxford, that provide synthesised information on ecological risk in the terrestrial and marine environment.

The tools serve geotemporal datasets through two approaches: 
1. generation of custom PDF reports and optional data downloads of the report contents; and 
2. a data packages API for custom data bundles.

Both data access methods require a local user account, but are currently free to use.

# Project Structure

There are two seperate apps in the `apps` folder. These both share a common set of libraries and a razor class library. In this way, webpage `.cshtml` templates are defined in the `src/web/Ecoset.WebUI` razor class library. Any page or layout can be overridden in the specific àpp projects (i.e. `Oxlel.MarineLEFT.WebUI` or `Oxlel.LEFT.WebUI`).

# Development and Deployment

For local development and deployment, the following dependencies are required:
* Docker 
* The .NET Core 3.0 SDK

Both applications are set up as Docker images. Each application folder contains a `docker-compose.yml`, which can be used to launch LEFT and all of its dependencies. With Docker installed, the following commands will build and launch LEFT on the local machine:

```bash
docker-compose -f docker-compose.yml -f docker-compose.development.yml build
docker-compose -f docker-compose.yml -f docker-compose.development.yml up
```

LEFT will then be running on localhost:5100 through an nginx reverse proxy.

Both LEFT and MarineLEFT use an Oxford-based MySQL mirror of the Global Biodiversity Information Facility. If a local copy is required for testing, this may also be run from within the `src/gbif/gbifdb` directory using `docker-compose up -d`.

Both LEFT and MarineLEFT may be deployed using a `docker-compose.production.yml`, which must be configured for your infrastructure. 

# Adding and Amending Datasets

This section states the required steps for tasks that may need to be completed during general maintainance of LEFT.

## Update a Dataset with a New Version

Ecoset is aware of time slices for datasets where the method and units have not changed since a previous release. For example, a model of vegetation greenness that uses remotely sensed data may be updated with newer observations. In these cases, LEFT will automatically detect new time slices or an extended temporal extent (depending on the Ecoset method used).

The way in which the new data is added will depend on the Ecoset method used. For built-in methods, refer to the below table.

| Method used | Time Type | How to add a new dataset |
| ----- | ----- | ----- |
| IntersectTIFF | Slices | Within the specified `tileDir`, add a subdirectory in the format `YYYY-MM-DD` and include tiled data in the common format, where only the year is required. Data can therefore be at yearly, monthly, or daily resolution. For example, the following are all valid: `2020`, `2020-02`, and `2020-02-29`. |
| IntersectShapefile | Slices | Create a subdirectory following the guidance for *IntersectTiff*. Within the new directory, place a single shapefile that has identical fields to previous time slices. |
| GbifQuery | Extent | Adding data into the mirrored copy of GBIF (MySQL database) will automatically lengthen the temporal extent reported and used. An update tool is included for this purpose (see seperate documentation). |

For PDF reports and data downloads, LEFT has been configured to always serve the latest dataset. Your changes will be reflected immediately. Alternatively, users of the data package API will be able to choose between the latest or historical datasets.

## Add a New Dataset or Method

There are three key stages to fully configure a new dataset in LEFT / Marine LEFT:
1. **Configure Ecoset with the new variable or method.** Follow the Ecoset documentation for this step.
2. **Add the variable / method into LEFT configuration.** This step is not required if you only wish to serve the data through the API, as it will be auto-discovered. Amend the `appsettings.json` file as follows:
    - To add the data in the PDF report, add a section as follows into the `FreeReportSections` array:
        ```json
        { "Variable": "variable_id",
          "Method": "method_id" }
        ```
    - To add the data in the optional download archive, add a section as follows into the `ProReportSections` array:
        ```json
        { "Variable": "variable_id",
          "Method": "method_id" }
        ```
3. **Modify the PDF template.** The razor file `GenerateReport.cshtml`must be amended to include the newly available data. Note that a javascript library is included for creating maps from raster or point data.

# Common Maintainance Tasks

LEFT should look after itself. However, if problems are encountered the following approaches may be used.

## Check Ecoset for errors

The Ecoset queue administration interface is available at {host}:5002/admin/queues. Reasons for analysis failure will be shown here in failed jobs. Common issues include missing or corrput datasets and misconfiguration of the GBIF MySQL database.

## Purge the Ecoset Cache

Ecoset keeps a copy of all previous outputs in the ècoset-cache folder designated in `docker-compose.yml`. Purging is not yet available. If the cache is manually emptied, you will no longer be able to regenerate the LEFT PDF outputs and zip files (only if they are deleted too) without resubmitting LEFT analyses.
