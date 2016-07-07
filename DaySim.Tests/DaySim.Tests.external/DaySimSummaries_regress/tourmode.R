###This script generates summaries for DaySim Tour mode models
###Distributions of tour modes by purpose are produced

print("Tour Mode Summary...Started")

prep_perdata <- function(perdata,hhdata)
{
  hhdata[,vehcat:=ifelse(hhvehs>0,1,0)]
  perdata <- merge(perdata,hhdata[,list(hhno,vehcat)],by="hhno",all.x=T)
  return(perdata)
}

prep_modedata_NHTS <- function(tourdata)
{
  tourdata[,tourmode:= 0]
  #Drive Alone
  tourdata[tmodetp==3,tourmode:= 1]
  #Shared Ride 2
  tourdata[tmodetp==4,tourmode:= 2]
  #Shared Ride 3+
  tourdata[tmodetp==5,tourmode:= 3]
  #Drive-Transit
  tourdata[tmodetp==7,tourmode:= 4]
  #Walk-Transit
  tourdata[tmodetp==6,tourmode:= 5]
  #Bike
  tourdata[tmodetp==2,tourmode:= 6]
  #Walk
  tourdata[tmodetp==1,tourmode:= 7]
  #School Bus
  tourdata[tmodetp==8,tourmode:= 8]  
  return(tourdata)
}

prep_modedata_DaySim <- function(tourdata)
{
  tourdata[,tourmode:= 0]
  #Drive Alone
  tourdata[tmodetp==3,tourmode:= 1]
  #Shared Ride 2
  tourdata[tmodetp==4,tourmode:= 2]
  #Shared Ride 3+
  tourdata[tmodetp==5,tourmode:= 3]
  #PNR
  tourdata[tmodetp==7 & (tpathtp==5|tpathtp==6),tourmode:= 4]
  #KNR
  tourdata[tmodetp==7 & (tpathtp==7|tpathtp==8),tourmode:= 5]
  #Walk-Transit
  tourdata[tmodetp==6,tourmode:= 6]
  #Bike
  tourdata[tmodetp==2,tourmode:= 7]
  #Walk
  tourdata[tmodetp==1,tourmode:= 8]
  #School Bus
  tourdata[tmodetp==8,tourmode:= 9]
  return(tourdata)
}

prep_tourdata <- function(tourdata,perdata)
{
  tourdata <- merge(tourdata,perdata,by=c("hhno","pno"),all.x=T)
  if(excludeChildren5)
    tourdata <- tourdata[pptyp<8]
  tourdata[pdpurp==8,pdpurp:=7]
  tourdata[pdpurp==9,pdpurp:=4]
  tourdata[,pdpurp2:=ifelse(parent == 0,pdpurp,8)]
  
  wrktours  <- tourdata[pdpurp == 1]
  wrktours <- wrktours[,c("hhno","pno","tour","tourmode"),with=F]
  setnames(wrktours,c("tourmode","tour"),c("parenttourmode","parent"))
  wrkbasedtours <- tourdata[parent > 0]
  wrkbasedtours <- merge(wrkbasedtours,wrktours,by=c("hhno","pno","parent"),all.x=T)
  
  nonwrkbasedtours <- tourdata[parent == 0]
  nonwrkbasedtours[,parenttourmode:= 0]
  wrkbasedtours <- wrkbasedtours[,names(nonwrkbasedtours),with=F]
  tourdata <- rbind(nonwrkbasedtours,wrkbasedtours)
  
  return(tourdata)
}

if(prepSurvey)
{
  survperdata <- assignLoad(paste0(surveyperfile,".Rdata"))
  survhhdata <- assignLoad(paste0(surveyhhfile,".Rdata"))
  survperdata <- prep_perdata(survperdata,survhhdata)
  survperdata <- survperdata[,c("hhno","pno","pptyp","vehcat","psexpfac"),with=F]
  rm(survhhdata)
  
  survtourdata <- assignLoad(paste0(surveytourfile,".Rdata"))
  survtourdata <- prep_modedata_NHTS(survtourdata)
  survtourdata <- prep_tourdata(survtourdata,survperdata)
  write_tables(tourmodemodelout,survtourdata,tourmodemodelfile,"reference")

  rm(survperdata,survtourdata)
  gc()
}

if(prepDaySim)
{
  
  dsperdata <- assignLoad(paste0(dsperfile,".Rdata"))
  dshhdata <- assignLoad(paste0(dshhfile,".Rdata"))
  dsperdata <- prep_perdata(dsperdata,dshhdata)
  dsperdata <- dsperdata[,c("hhno","pno","pptyp","vehcat","psexpfac"),with=F]
  rm(dshhdata)
  
  dstourdata <- assignLoad(paste0(dstourfile,".Rdata"))
  dstourdata <- prep_modedata_NHTS(dstourdata) #prep_modedata_DaySim is only for Tampa/JAX
  dstourdata <- prep_tourdata(dstourdata,dsperdata)
  write_tables(tourmodemodelout,dstourdata,tourmodemodelfile,"new")
  
  rm(dsperdata,dstourdata)
  gc()
}


print("Tour Mode Summary...Finished")
