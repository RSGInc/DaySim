###This script generates summaries for NHTS and DaySim Tour Time fo Day models
###Distributions of tour arrival and departure times along with those of durations at tour destinations are produced

print("Tour Time of Day Summary...Started")

prep_tourdata <- function(tourdata,perdata)
{
  tourdata <- merge(tourdata,perdata[,list(hhno,pno,pptyp,psexpfac)],by=c("hhno","pno"),all.x=T)
  if(excludeChildren5)
    tourdata <- tourdata[pptyp<8]
  tourdata[pdpurp>0,pdpurp2:=c(1,2,3,3,3,3,3)[pdpurp]]
  tourdata[pdpurp==0,pdpurp2:=0]
  tourdata[parent>0,pdpurp2:=4]
  
  tourdata[,deppdhr:= trunc(tlvdest/100)]
  tourdata[,deppdmin:= tlvdest - deppdhr*100]
  if(length(tourdata$deppdmin[tourdata$deppdmin>60])>0)
  {
    tourdata[,deptime:= tlvdest/60]
    tourdata[,arrtime:= tardest/60]
  } else{
    tourdata[,deptime:= deppdhr + deppdmin/60]
    tourdata[,arrpdhr:= trunc(tardest/100)]
    tourdata[,arrpdmin:= tardest - arrpdhr*100]
    tourdata[,arrtime:= arrpdhr + arrpdmin/60]
  }
  tourdata[,durdest:= deptime - arrtime]
  
  cats <- c(0,seq(3,27.5,.5))
  tourdata[,arrtimecat:=findInterval(arrtime,cats)]
  tourdata[,deptimecat:=findInterval(deptime,cats)]
  cats <- c(0,seq(0.5,24,.5))
  tourdata[,durdestcat:=findInterval(durdest,cats)]
  
  return(tourdata)
}

if(prepSurvey)
{
  survperdata <- assignLoad(paste0(surveyperfile,".Rdata"))
  survtourdata <- assignLoad(paste0(surveytourfile,".Rdata"))
  survtourdata <- prep_tourdata(survtourdata,survperdata)
  write_tables(tourtodmodelout,survtourdata,tourtodmodelfile,"reference")

  rm(survperdata,survtourdata)
  gc()
}

if(prepDaySim)
{
  dsperdata <- assignLoad(paste0(dsperfile,".Rdata"))
  dstourdata <- assignLoad(paste0(dstourfile,".Rdata"))
  dstourdata <- prep_tourdata(dstourdata,dsperdata)
  write_tables(tourtodmodelout,dstourdata,tourtodmodelfile,"new")
  
  rm(dsperdata,dstourdata)
  gc()
}

print("Tour Time of Day Summary...Finished")
