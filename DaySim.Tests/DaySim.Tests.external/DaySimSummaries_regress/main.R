##This is a temporary main control file with lots of TODOs to move
##this whole effort to a package with project specific config and run files

#Rprof()

options(warn = 1) #print warnings as they occur
## This will print the stack trace at the time of the error.
options(error = function() traceback(2))
options(warning = function() traceback(2))

#-----------------------
# Load packages
#-----------------------

safeLoadPackage <- function(package_name){
  tryCatch({
    library(package_name, character.only=TRUE)
  }, error = function(err) {
    print(paste("Installing", package_name))
    install.packages(package_name, repos="https://ftp.osuosl.org/pub/cran")
    library(package_name, character.only=TRUE)
  })
}

safeLoadPackage("foreign")
safeLoadPackage("reshape")
safeLoadPackage("XLConnect")
safeLoadPackage("descr")
safeLoadPackage("Hmisc")
safeLoadPackage("data.table")
safeLoadPackage("plyr")

getScriptDirectory <- function() {
  argv <- commandArgs(trailingOnly = FALSE)
  script_dir <- dirname(substring(argv[grep("--file=", argv)], 8))
  print(paste('script_dir:', script_dir))
  return(script_dir)
}

print('sys.frames')
print(sys.frames)
print('commandArgs(FALSE)')
print(commandArgs(FALSE))

setScriptDirectory <- function() {
  script_dir <- getScriptDirectory()
  if (length(script_dir) == 0) {
    print("WARNING: script directory could not be found so leaving current working directory")
  }
  else {
    setwd(script_dir)
  }
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

getNamedArg <- function(args, arg_name, default) {
  named_arg = substring(args[grep(arg_name, args)], nchar(arg_name) + 1)
  if ((!is.character(named_arg)) | (length(named_arg) == 0)) {
    print(paste("No", arg_name, "specified. Using default value."))
    return(default)
  } else {
    return(named_arg)
  }
}


#------------------------------------
# Source functions and config settings
#------------------------------------
# this script uses relative paths for sourcing so requires that the current working directory be script directory
setScriptDirectory()

args <- commandArgs(trailingOnly=TRUE)
print(paste('# of args=',length(args), 'Args:'))
print(args)

# Override defaults if specified in commandArgs
DAYSIM_REFERENCE_OUTPUTS          = getNamedArg(args, "--reference_dir=", "reference/regress_outputs")
DAYSIM_NEW_OUTPUTS                = getNamedArg(args, "--outputs_dir=", "new/regress_outputs")
DAYSIM_REPORT_DIRECTORY           = getNamedArg(args, "--reports_dir=", "excel_report_files")

print(paste("DAYSIM_REFERENCE_OUTPUTS:", DAYSIM_REFERENCE_OUTPUTS))
print(paste("DAYSIM_NEW_OUTPUTS:", DAYSIM_NEW_OUTPUTS))
print(paste("DAYSIM_REPORT_DIRECTORY:", DAYSIM_REPORT_DIRECTORY))

# DaySim Version - DelPhi or C#
dsVersion                         = "C#"

# daysim outputs
dshhfile                          = paste(DAYSIM_NEW_OUTPUTS, "/_household.tsv", sep="")
dsperfile                         = paste(DAYSIM_NEW_OUTPUTS, "/_person.tsv", sep="")
dspdayfile                        = paste(DAYSIM_NEW_OUTPUTS, "/_person_day.tsv", sep="")
dstourfile                        = paste(DAYSIM_NEW_OUTPUTS, "/_tour.tsv", sep="")
dstripfile                        = paste(DAYSIM_NEW_OUTPUTS, "/_trip.tsv", sep="")

# reference/survey
surveyhhfile                      = paste(DAYSIM_REFERENCE_OUTPUTS, "/_household.tsv", sep="")
surveyperfile                     = paste(DAYSIM_REFERENCE_OUTPUTS, "/_person.tsv", sep="")
surveypdayfile                    = paste(DAYSIM_REFERENCE_OUTPUTS, "/_person_day.tsv", sep="")
surveytourfile                    = paste(DAYSIM_REFERENCE_OUTPUTS, "/_tour.tsv", sep="")
surveytripfile                    = paste(DAYSIM_REFERENCE_OUTPUTS, "/_trip.tsv", sep="")

wrklocmodelfile                   = "./model_csv_files/WrkLocation.csv"
schlocmodelfile                   = "./model_csv_files/SchLocation.csv"
vehavmodelfile                    = "./model_csv_files/VehAvailability.csv"
daypatmodelfile1                  = "./model_csv_files/DayPattern_pday.csv"
daypatmodelfile2                  = "./model_csv_files/DayPattern_tour.csv"
daypatmodelfile3                  = "./model_csv_files/DayPattern_trip.csv"
tourdestmodelfile                 = "./model_csv_files/TourDestination.csv"
tourdestwkbmodelfile              = "./model_csv_files/TourDestination_wkbased.csv"
tripdestmodelfile                 = "./model_csv_files/TripDestination.csv"
tourmodemodelfile                 = "./model_csv_files/TourMode.csv"
tourtodmodelfile                  = "./model_csv_files/TourTOD.csv"
tripmodemodelfile                 = "./model_csv_files/TripMode.csv"
triptodmodelfile                  = "./model_csv_files/TripTOD.csv"

wrklocmodelout                    = "WrkLocation.xlsm"
schlocmodelout                    = "SchLocation.xlsm"
vehavmodelout                     = "VehAvailability.xlsm"
daypatmodelout                    = "DayPattern.xlsm"
tourdestmodelout                  = c("TourDestination_Escort.xlsm","TourDestination_PerBus.xlsm","TourDestination_Shop.xlsm",
                                      "TourDestination_Meal.xlsm","TourDestination_SocRec.xlsm")
tourdestwkbmodelout               = "TourDestination_WrkBased.xlsm"
tourmodemodelout                  = "TourMode.xlsm"
tourtodmodelout                   = "TourTOD.xlsm"
tripmodemodelout                  = "TripMode.xlsm"
triptodmodelout                   = "TripTOD.xlsm"

outputsDir                        = paste(DAYSIM_REPORT_DIRECTORY, "", sep="")
validationDir                     = ""

prepSurvey                        = TRUE
prepDaySim                        = TRUE

runWrkSchLocationChoice           = TRUE
runVehAvailability                = TRUE
runDayPattern                     = TRUE
runTourDestination                = TRUE
runTourMode                       = TRUE
runTourTOD                        = TRUE
runTripMode                       = TRUE
runTripTOD                        = TRUE

excludeChildren5                  = TRUE

sourceAFileInTryCatch("utilfunc.R")

progressStart("run DaySim summaries",14)

#-----------------------
# Load data
#-----------------------

# Load DaySim outputs into Rdata files
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

#force gc()
gc()

#-----------------------
# Run tabulations
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
