function AdminViewModel(users, analyses) {
    let self = this;
    self.analyses = ko.observableArray([]);
    self.users = ko.observableArray([]);
    self.dataPackages = ko.observableArray([]);

    self.problemCount = ko.computed(function () {
        let count = 0;
        ko.utils.arrayForEach(self.analyses(), function (analysis) {
            if (analysis.status == "Failed") count = count + 1;
        });
        return count;
    });

    self.inProgressCount = ko.computed(function () {
        let count = 0;
        ko.utils.arrayForEach(self.analyses(), function (analysis) {
            if (analysis.status != "Failed" && analysis.status != "Completed") count = count + 1;
        });
        return count;
    });

    self.inProgress = ko.computed(function () {
        return ko.utils.arrayFilter(self.analyses(), function (analysis) {
            return analysis.status != "Failed" && analysis.status != "Completed";
        });
    });

    self.problems = ko.computed(function () {
        return ko.utils.arrayFilter(self.analyses(), function (analysis) {
            return analysis.status == "Failed";
        });
    });

    self.restartAnalysis = function (analysis) {
        if (confirm('Restart analysis?')) {
            console.log('restarting analysis #' + analysis.id);
            $.ajax({
                url: '/Administration/Home/RestartJob?jobId=' + analysis.id,
                cache: false,
                success: function (updatedJob) {
                    console.log('Job was restarted');
                    self.updateAnalyses;
                }
            })
        }
    }

    self.restartPro = function (analysis) {
        if (confirm('Restart pro data generation?')) {
            console.log('restarting pro data for #' + analysis.id);
            $.ajax({
                url: '/Administration/Home/RestartPro?jobId=' + analysis.id,
                cache: false,
                success: function (updatedJob) {
                    console.log('Job was restarted');
                    self.updateAnalyses;
                }
            })
        }
    }

    self.hide = function (analysis) {
        if (confirm('Hiding this analysis will remove it from both the user account and the admin panel. Continue?')) {
            console.log('hiding analysis #' + analysis.id);
            $.ajax({
                url: '/Administration/Home/HideAnalysis?id=' + analysis.id,
                cache: false,
                success: function (updatedJob) {
                    self.updateAnalyses;
                }
            })
        }
    }

    self.stopAnalysis = function (analysis) {
        console.log('stopping analysis #' + analysis.id);
        $.ajax({
            url: '/Administration/Home/StopJob?jobId=' + analysis.id,
            cache: false,
            success: function (updatedJob) {
                console.log('Job was stopped');
                self.updateAnalyses;
            }
        })
    }

    self.updateAnalyses = function () {
        $.ajax({
            url: "/Administration/Home/GetJobs",
            cache: false,
            success: function (serverAnalyses) {
                self.analyses(serverAnalyses);
            }
        });
    }

    self.updatePackages = function () {
        $.ajax({
            url: "/Administration/Home/GetPackages",
            cache: false,
            success: function (serverAnalyses) {
                self.dataPackages(serverAnalyses);
            }
        });
    }

    self.updateUsers = function () {
        $.ajax({
            url: "/Administration/Home/GetUsers",
            cache: false,
            success: function (users) {
                self.users(users);
            }
        });
    }

    self.addCredits = function (credits, user) {
        if (confirm('Give ' + credits + ' *free* credits to ' + user.userName + '?')) {
            $.ajax({
                url: '/Administration/Home/AddCredits?userId=' + user.id + '&credits=' + credits,
                cache: false,
                success: function (updatedUser) {
                    console.log('Gave ' + credits + ' free credit(s) to ' + user.userName);
                    self.updateUsers;
                }
            })
        }
    }

    self.makeAdmin = function (user) {
        if (confirm('are you sure you want to make ' + user.userName + ' a LEFT administrator?')) {
            $.ajax({
                url: '/Administration/Home/UserAdmin?id=' + user.id + '&userIsAdmin=true',
                cache: false,
                success: function (updatedUser) {
                    console.log('User was made admin: ' + updatedUser.userName);
                    self.updateUsers;
                }
            })
        }
    }

}

$(document).ready(function () {
    var vm = new AdminViewModel();
    window.setInterval(vm.updateAnalyses, 10000);
    window.setInterval(vm.updatePackages, 30000);
    window.setInterval(vm.updateUsers, 60000 * 5);
    vm.updateUsers();
    vm.updateAnalyses();
    vm.updatePackages();
    ko.applyBindings(vm);
});