##This is a temporary main control file with lots of TODOs to move 
##this whole effort to a package with project specific config and run files

#Rprof()

options(warn = 2) #treat warnings like errors

## This will print the stack trace at the time of the error.
options(error = function() traceback())
options(warning = function() traceback())

#-----------------------
#Load packages
#-----------------------
##TODO consider local R installation, with predownloaded packages in that library
library(foreign)
library(reshape)
library(XLConnect)
library(descr)
library(Hmisc)
library(data.table)
library(plyr)

getScriptDirectory <- function() {
  frame_files_initial <- lapply(sys.frames(), function(x) x$ofile)
  frame_files_filtered <- Filter(Negate(is.null), frame_files_initial)
  num_frame_files_filtered = length(frame_files_filtered)
  print(paste('num_frame_files_filtered:', num_frame_files_filtered))
  if (num_frame_files_filtered == 0) {
    stop("Can not get script directory")
  } else {
    script_dir <- dirname(frame_files_filtered[[length(frame_files_filtered)]])
  }
  
  #argv <- commandArgs(trailingOnly = FALSE)
  #script_dir <- dirname(substring(argv[grep("--file=", argv)], 8))
  print(paste('script_dir:', script_dir))
  return(script_dir)
}

print('sys.frames')
print(sys.frames)
print('commandArgs(FALSE)')
print(commandArgs(FALSE))

setScriptDirectory <- function() {
  script_dir <- getScriptDirectory()
  setwd(script_dir)
}

#this script uses relative paths for sourcing so requires that the current working directory be script directory
setScriptDirectory()

#Tried to use ArgParse but it screwed up the long filename of the config file
args <- commandArgs(trailingOnly=FALSE)
if (length(args) == 1) {
  print(paste('no extra arguments beyond script name:', args[1], 'so using default configuration file.'))
  configuration_file = 'daysim_output_config.R'
} else if (length(args) > 2) {
  print(paste('Unexpected arguments. Expect only path to configuration file but found more than one argument. Number of args:', length(args)))
} else {
  configuration_file = args[2]
  #remove quotes if needed
  configuration_file <- gsub('[\'"]', '', configuration_file)
  print(paste("configuration_file:", configuration_file))
}

sourceAFileInTryCatch <- function(filename){
  if (!file.exists(filename)) {
    stop(paste('Expected source file does not exist:', filename))
  }
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
sourceAFileInTryCatch(configuration_file)

#stop("Finished test")

sourceAFileInTryCatch("utilfunc.R")

progressStart("run DaySim summaries",14)

#-----------------------
#Load data
#-----------------------

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

  progressNextStep("reading tour weight adjustment")
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