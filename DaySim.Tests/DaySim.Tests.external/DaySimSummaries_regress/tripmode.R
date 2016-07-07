###This script generates summaries for DaySim Trip mode models
###Distributions of trip mode by tour mode are produced

print("Trip Mode Summary...Started")

prep_tomode_NHTS <- function(tourdata)
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

prep_tomode_DaySim <- function(tourdata)
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
prep_trmode_DaySim <- function(tripdata)
{
  tripdata[,tripmode:= 0]
  #Drive Alone
  tripdata[mode==3,tripmode:= 1]
  #Shared Ride 2
  tripdata[mode==4,tripmode:= 2]
  #Shared Ride 3+ 
  tripdata[mode==5,tripmode:= 3]
  #Transit - bus
  tripdata[mode==6 & pathtype %in% c(3,5,7),tripmode:= 4]
  #Transit - project/CR
  tripdata[mode==6 & pathtype %in% c(4,6,8),tripmode:= 5]
  #Transit - NA/Place holder
  tripdata[mode==6 & pathtype>9,tripmode:= 6]
  #Transit - commuter rail
  tripdata[mode==6 & pathtype>9,tripmode:= 7]
  #Transit - ferry
  tripdata[mode==6 & pathtype>9,tripmode:= 8]
  #School Bus
  tripdata[mode==8,tripmode:= 9]
  #Bike
  tripdata[mode==2,tripmode:= 10]
  #Walk
  tripdata[mode==1,tripmode:= 11]
  
  return(tripdata)
}

prep_trmode_NHTS <- function(tripdata)
{
  tripdata[,tripmode:= 0]
  #Drive Alone
  tripdata[mode==3,tripmode:= 1]
  #Shared Ride 2
  tripdata[mode==4,tripmode:= 2]
  #Shared Ride 3+ 
  tripdata[mode==5,tripmode:= 3]
  #Transit - localbus
  tripdata[mode==6 & pathtype==3,tripmode:= 4]
  #Transit - lightrail
  tripdata[mode==6 & pathtype==4,tripmode:= 5]
  #Transit - premium bus
  tripdata[mode==6 & pathtype==5,tripmode:= 6]
  #Transit - commuter rail
  tripdata[mode==6 & pathtype==6,tripmode:= 7]
  #Transit - ferry
  tripdata[mode==6 & pathtype==7,tripmode:= 8]
  #School Bus
  tripdata[mode==8,tripmode:= 9]
  #Bike
  tripdata[mode==2,tripmode:= 10]
  #Walk
  tripdata[mode==1,tripmode:= 11]

  return(tripdata)
}


prep_tripdata <- function(tripdata,tourdata,perdata,prepsrc)
{
  tourdata <- merge(tourdata,perdata[,list(hhno,pno,pptyp,psexpfac)],by=c("hhno","pno"),all.x=T)
  tourdata[,pdpurp2:=ifelse(parent == 0,pdpurp,8)]
  
  if(prepsrc=="reference")
  {
    tourdata <- prep_tomode_NHTS(tourdata)
    tripdata <- prep_trmode_NHTS(tripdata)
  } 
  if(prepsrc=="new")
  {
    #prep_tomode_DaySim and prep_trmode_DaySim are only for Tampa/JAX
    tourdata <- prep_tomode_NHTS(tourdata)
    tripdata <- prep_trmode_NHTS(tripdata)
  }
  tourdata <- tourdata[,list(hhno,pno,tour,tourmode,pdpurp2,pptyp,psexpfac)]
  tripdata <- merge(tripdata,tourdata,by=c("hhno","pno","tour"),all.x=T)
  if(excludeChildren5)
    tripdata <- tripdata[pptyp<8]
  return(tripdata)
}

if(prepSurvey)
{
  survperdata <- assignLoad(paste0(surveyperfile,".Rdata"))
  survtourdata <- assignLoad(paste0(surveytourfile,".Rdata"))
  survtripdata <- assignLoad(paste0(surveytripfile,".Rdata"))
  survtripdata <- prep_tripdata(survtripdata,survtourdata,survperdata,"reference")
  write_tables(tripmodemodelout,survtripdata,tripmodemodelfile,"reference")
  
  rm(survperdata,survtourdata,survtripdata)
  gc()
}

if(prepDaySim)
{
  
  dsperdata <- assignLoad(paste0(dsperfile,".Rdata"))
  dstourdata <- assignLoad(paste0(dstourfile,".Rdata"))
  dstripdata <- assignLoad(paste0(dstripfile,".Rdata"))
  dstripdata <- prep_tripdata(dstripdata,dstourdata,dsperdata,"new")
  write_tables(tripmodemodelout,dstripdata,tripmodemodelfile,"new")

  rm(dsperdata,dstourdata,dstripdata)
  gc()
}

print("Trip Mode Summary...Finished")