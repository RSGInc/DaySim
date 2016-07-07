###This script generates Trip Destination Summaries from DaySim run outputs

print("Trip Destination Summary...Started")

prep_perdata <- function(perdata,hhdata)
{
  hhdata[,hhcounty:=1] #county set to 1
  hhdata <-hhdata[,list(hhno,hhtaz,hhcounty)]
  perdata <- merge(perdata,hhdata,by="hhno",all.x=T)
  return(perdata)
}

prep_tripdata <- function(tripdata,perdata)
{
  tripdata <- merge(tripdata,perdata,by=c("hhno","pno"),all.x=T)
  if(excludeChildren5)
    tripdata <- tripdata[pptyp<8]
  tripdata[,ocounty:=1] #county set to 1
  tripdata[,dcounty:=1] #county set to 1
  
  if(sum(tripdata$travdist,na.rm=T)==0)
  {
    print("Error!!! - Skims information missing.")
  }
  tripdata[,distcat:=findInterval(travdist,0:90)]
  tripdata[,timecat:=findInterval(travtime,0:90)]
  tripdata[,wrkrtyp:=c(1,2,3,3,3,3,3,3)[pptyp]]
  tripdata[travtime<0,travtime:=NA]
  tripdata[travdist<0,travdist:=NA]

  return(tripdata)
}

suff <- c("Home","Work","School","Escort","PerBus","Shop","Meal","SocRec")
if(prepSurvey)
{
  survperdata <- assignLoad(paste0(surveyperfile,".Rdata"))
  survhhdata <- assignLoad(paste0(surveyhhfile,".Rdata"))
  survperdata <- prep_perdata(survperdata,survhhdata)
  survperdata <- survperdata[,c("hhno","pno","pptyp","hhtaz","hhcounty","pwtaz","psexpfac"),with=F]
  rm(survhhdata)
  
  survtripdestmodfile <- read.table(tripdestmodelfile,header=T,sep=",")
  survtripdestmodfile$outsheet_orig <- survtripdestmodfile$outsheet
  
  survtripdata <- assignLoad(paste0(surveytripfile,".Rdata"))
  survtripdata <- prep_tripdata(survtripdata,survperdata)

  wb = loadWorkbook(paste(outputsDir,"/TripDestination.xlsm",sep=""))
  setStyleAction(wb,XLC$"STYLE_ACTION.NONE")
  for(i in 1:length(suff))
  {
    purp <- i-1  
    survtripdestmodfile$outsheet <- paste(survtripdestmodfile$outsheet_orig,suff[i],sep="_")
    tabulate_summaries(survtripdata[survtripdata$dpurp == purp],survtripdestmodfile,"reference",wb)
  }
  saveWorkbook(wb)
  
  rm(survtripdestmodfile,survperdata,survtripdata)
  gc()
}

if(prepDaySim)
{
  dsperdata <- assignLoad(paste0(dsperfile,".Rdata"))
  dshhdata <- assignLoad(paste0(dshhfile,".Rdata"))
  dsperdata <- prep_perdata(dsperdata,dshhdata)
  dsperdata <- dsperdata[,c("hhno","pno","pptyp","hhtaz","hhcounty","pwtaz","psexpfac"),with=F]
  rm(dshhdata)
  
  dstripdestmodfile <- read.table(tripdestmodelfile,header=T,sep=",")
  dstripdestmodfile$outsheet_orig <- dstripdestmodfile$outsheet
  
  dstripdata <- assignLoad(paste0(dstripfile,".Rdata"))
  dstripdata <- prep_tripdata(dstripdata,dsperdata)
  
  wb = loadWorkbook(paste(outputsDir,"/TripDestination.xlsm",sep=""))
  setStyleAction(wb,XLC$"STYLE_ACTION.NONE")
  for(i in 1:length(suff))
  {
    purp <- i-1  
    dstripdestmodfile$outsheet <- paste(dstripdestmodfile$outsheet_orig,suff[i],sep="_")
    tabulate_summaries(dstripdata[dstripdata$dpurp == purp],dstripdestmodfile,"new",wb)
  }
  saveWorkbook(wb)
  
  rm(dstripdestmodfile,dsperdata,dstripdata)
  gc()
}

print("Trip Destination Summary...Finished")