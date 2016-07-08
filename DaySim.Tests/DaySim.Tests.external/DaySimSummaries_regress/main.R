##This is a temporary main control file with lots of TODOs to move 
##this whole effort to a package with project specific config and run files

#Rprof()

## This will print the stack trace at the time of the error.
options(error = function() traceback(2))
options(warning = function() traceback(2))

#-----------------------
#Load packages
#-----------------------
##TODO consider local R installation, with predownloaded packages in that library
library(argparse)
library(foreign)
library(reshape)
library(XLConnect)
library(descr)
library(Hmisc)
library(data.table)
library(plyr)
library( rhdf5 ) #PSRC HDF5

#this script assumes current working directory in set to script location
this.dir <- dirname(parent.frame(2)$ofile)
setwd(this.dir)

paste("commandArgs(TRUE)", commandArgs(TRUE))

#read in command line arguments
parser <-ArgumentParser(description='Compare two DaySim output directories')
parser$add_argument('--configuration_file', help='The reference saved outputs from a successful run [default: %(default)s]', default='daysim_output_config.R')
parser$add_argument('--daysim_outputs_reference', help='The reference saved outputs from a successful run', default='')
parser$add_argument('--daysim_outputs_new', help='Newly generated result to be compared to reference', default='')
parser$add_argument('--report_directory', help='will create or replace as needed', default='')
parser$add_argument('-v', '--verbose', help='increase output verbosity',
                    action='store_true')
args <- parser$parse_args(commandArgs(TRUE))

#parser too stupid to remove quotes
print(cat("args$configuration_file", args$configuration_file))
args$configuration_file <- gsub('[\'"]', '', args$configuration_file)
print(cat("args$configuration_file", args$configuration_file))

#print(args)

DAYSIM_REFERENCE_OUTPUTS <- args$daysim_outputs_reference
DAYSIM_NEW_OUTPUTS <- args$daysim_outputs_new
DAYSIM_REPORT_DIRECTORY <- args$report_directory

sourceAFileInTryCatch <- function(filename){
  tryCatch({
    source(filename)
  }, warning = function(war) {
    print(paste("Caught WARNING while sourcing:", filename, "Warning:",war))
    stop()
  }, error = function(err) {
    print(paste("Caught ERROR while sourcing:", filename, "error:",err))
    stop()
  })
}


#------------------------------------
#Source functions and config settings
#------------------------------------
#TODO function in package to create template config file in a specified location
sourceAFileInTryCatch(args$configuration_file)

#stop("Finished test")

sourceAFileInTryCatch("utilfunc.R")

progressStart("run DaySim summaries",14)

#-----------------------
#Load data
#-----------------------

#Geographical correspondence
countycorr <- fread(tazcountycorr)

#Load DaySim outputs into Rdata files
if(runWrkSchLocationChoice | runVehAvailability | runDayPattern | runTourDestination | runTourMode)
{
  progressNextStep("reading hh data")
  if(prepDaySim)
    readSaveRdata(dshhfile,"dshhdata")
  if(prepSurvey)
    readSaveRdata(surveyhhfile,"survhhdata")
}

if(runWrkSchLocationChoice | runDayPattern | runTourDestination | runTourMode | runTourTOD | runTripMode | runTripTOD)
{
  progressNextStep("reading person data")
  if(prepDaySim)
    readSaveRdata(dsperfile,"dsperdata")
  if(prepSurvey)
    readSaveRdata(surveyperfile,"survperdata")
}

if(runDayPattern)
{
  progressNextStep("reading person day data")
  if(prepDaySim)
    readSaveRdata(dspdayfile,"dspdaydata")
  if(prepSurvey)
    readSaveRdata(surveypdayfile,"survpdaydata")
}

if(runDayPattern | runTourDestination | runTourMode | runTourTOD | runTripMode)
{
  progressNextStep("reading person day tour data")
  if(prepDaySim)
    readSaveRdata(dstourfile,"dstourdata")
  if(prepSurvey)
    readSaveRdata(surveytourfile,"survtourdata")
}

if(runDayPattern | runTripMode | runTripTOD)
{
  progressNextStep("reading person day trip data")
  if(prepDaySim)
    readSaveRdata(dstripfile,"dstripdata")
  if(prepSurvey)
    readSaveRdata(surveytripfile,"survtripdata")
}

#Optional tour weight adjustment
if(tourAdj)
{
  progressNextStep("reading tour weight adjustment")
  touradj <- fread(tourAdjFile)
}

#force gc()
gc()

#-----------------------
#Run tabulations
#-----------------------
##TODO split between preparing tables in an R object and then putting them somewhere
##TODO e.g. in a spreadsheet, in a pdf report, etc.

#source("nonhwy.R")

if(runWrkSchLocationChoice)
{
  progressNextStep("summarizing work location choice")
  sourceAFileInTryCatch("wrkschlocation.R")
}
if(runVehAvailability)
{
  progressNextStep("summarizing vehicle ownership choice")
  sourceAFileInTryCatch("vehavailability.R")
}
if(runDayPattern)
{
  progressNextStep("summarizing Day pattern")
  sourceAFileInTryCatch("daypattern.R")
}
if(runTourDestination)
{
  progressNextStep("summarizing Destination Choice")
  sourceAFileInTryCatch("tourDestination.R")
}
if(runTourDestination)
{
  progressNextStep("summarizing Trip Destination Choice")
  sourceAFileInTryCatch("tripdestination.R")
}
if(runTourMode)
{
  progressNextStep("summarizing Tour Mode Choice") 
  sourceAFileInTryCatch("tourmode.R")
}
if(runTourTOD)
{
  progressNextStep("summarizing Tour Time of Day Choice") 
  sourceAFileInTryCatch("tourtod.R")
}
if(runTripMode)
{
  progressNextStep("summarizing Trip Mode Choice") 
  sourceAFileInTryCatch("tripmode.R")
}
if(runTripTOD)
{
  progressNextStep("summarizing Trip Time of Day Choice")
  sourceAFileInTryCatch("triptod.R")
}

progressEnd(outputsDir)

# Rprof(NULL)
# memprof <- summaryRprof()