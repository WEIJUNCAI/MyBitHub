/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    lodash = require("lodash");

var rootPaths = {
    webroot: "./wwwroot/",
    nodeDependencyRoot: "../node_modules/"
};

var paths = {};

paths.js = rootPaths.webroot + "js/**/*.js";
paths.minJs = rootPaths.webroot + "js/**/*.min.js";
paths.css = rootPaths.webroot + "css/**/*.css";
paths.minCss = rootPaths.webroot + "css/**/*.min.css";
paths.concatJsDest = rootPaths.webroot + "js/site.min.js";
paths.concatCssDest = rootPaths.webroot + "css/site.min.css";

var nodeDependenciesJs = [

    rootPaths.nodeDependencyRoot + "diff2html/dist/diff2html.js",
    rootPaths.nodeDependencyRoot + "diff2html/dist/diff2html-ui.js",
    rootPaths.nodeDependencyRoot + "diff/dist/diff.js",
    rootPaths.nodeDependencyRoot + "ace-builds/src-noconflict/*.js"
];

var nodeDependenciesCss = [

    rootPaths.nodeDependencyRoot + "diff2html/dist/diff2html.css"
]


gulp.task("clean:js", function (cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("min:js", function () {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:css", function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("min", ["min:js", "min:css"]);


gulp.task('copy-js', function () {
    return gulp.src(nodeDependenciesJs)
        .pipe(gulp.dest("./wwwroot/js"));
});

gulp.task('copy-css', function () {
    return gulp.src(nodeDependenciesCss)
        .pipe(gulp.dest("./wwwroot/css"));
});