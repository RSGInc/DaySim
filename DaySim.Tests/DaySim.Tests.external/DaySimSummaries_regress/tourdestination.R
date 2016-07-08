###This script generates Day Patterns from DaySim run outputs

print("Tour Destination Summary...Started")

prep_perdata <- function(perdata,hhdata)
{
  hhdata[,hhcounty:=1] #county set to 1
  perdata <- merge(perdata,hhdata,by="hhno",all.x=T)
  return(perdata)
}

prep_tourdata <- function(tourdata,perdata)
{
  tourdata <- merge(tourdata,perdata,by=c("hhno","pno"),all.x=T)
  if(excludeChildren5)
    tourdata <- tourdata[pptyp<8,]
  tourdata[pdpurp==8,pdpurp:=7]
  tourdata[pdpurp==9,pdpurp:=4]
  tourdata[,pdpurp2:=ifelse(parent == 0,pdpurp,8)]
  tourdata[,ocounty:=1] #county set to 1
  tourdata[,dcounty:=1] #county set to 1
  
  tourdata[,distcat:=findInterval(tautodist,0:90)]
  tourdata[,timecat:=findInterval(tautotime,0:90)]
  tourdata[,wrkrtyp:=c(1,2,3,3,3,3,3,3)[pptyp]]
  tourdata[tautotime<0,tautotime:=NA]
  tourdata[tautodist<0,tautodist:=NA]

  return(tourdata)
}

suff <- c("Escort","PerBus","Shop","Meal","SocRec")
if(prepSurvey)
{
  survperdata <- assignLoad(paste0(surveyperfile,".Rdata"))
  survhhdata <- assignLoad(paste0(surveyhhfile,".Rdata"))
  survperdata <- prep_perdata(survperdata,survhhdata)
  survperdata <- survperdata[,c("hhno","pno","pptyp","hhtaz","hhcounty","pwtaz","psexpfac"),with=F]
  rm(survhhdata)
  
  survtourdata <- assignLoad(paste0(surveytourfile,".Rdata"))
  survtourdata <- prep_tourdata(survtourdata,survperdata)

  for(i in 1:length(suff))
  {
    purp <- i+2 
    write_tables(tourdestmodelout[i],survtourdata[pdpurp2 == purp],tourdestmodelfile,"reference")
  }
  purp <- 8
  write_tables(tourdestwkbmodelout,survtourdata[pdpurp2 == purp],tourdestwkbmodelfile,"reference")
  
  rm(survperdata,survtourdata)
  gc()
}

if(prepDaySim)
{
  dsperdata <- assignLoad(paste0(dsperfile,".Rdata"))
  dshhdata <- assignLoad(paste0(dshhfile,".Rdata"))
  dsperdata <- prep_perdata(dsperdata,dshhdata)
  dsperdata <- dsperdata[,c("hhno","pno","pptyp","hhtaz","hhcounty","pwtaz","psexpfac"),with=F]
  rm(dshhdata)
  
  dstourdata <- assignLoad(paste0(dstourfile,".Rdata"))
  dstourdata <- prep_tourdata(dstourdata,dsperdata)

  for(i in 1:length(suff))
  {
    purp <- i+2 
    write_tables(tourdestmodelout[i],dstourdata[pdpurp2 == purp],tourdestmodelfile,"new")
  }
  purp <- 8
  write_tables(tourdestwkbmodelout,dstourdata[pdpurp2 == purp],tourdestwkbmodelfile,"new")

  rm(dsperdata,dstourdata)
  gc()
}

print("Tour Destination Summary...Finished")