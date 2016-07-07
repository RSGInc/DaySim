###This script generates summaries for DaySim Trip Time fo Day models
###Distributions of trip arrival and departure times along with those of durations at trip destinations are produced

print("Trip Time of Day Summary...Started")

prep_tripdata <- function(tripdata,perdata)
{
  tripdata <- merge(tripdata,perdata[,list(hhno,pno,pptyp,psexpfac)],by=c("hhno","pno"),all.x=T)
  if(excludeChildren5)
    tripdata <- tripdata[pptyp<8]
  
  tripdata[,dephr:= trunc(deptm/100)]
  tripdata[,depmin:= deptm - dephr*100]
  if(length(tripdata$depmin[tripdata$depmin>60])>0)
  {
    tripdata[,deptime:= deptm/60]
    tripdata[,arrtime:= arrtm/60]
    tripdata[,durdest:= endacttm - arrtm]
  } else{
    tripdata[,deptime:= dephr + depmin/60]
    tripdata[,arrhr:= trunc(arrtm/100)]
    tripdata[,arrmin:= arrtm - arrhr*100]
    tripdata[,arrtime:= arrhr + arrmin/60]
    tripdata[,durdest:= (trunc(endacttm/100)-trunc(arrtm/100))*60 +
      (endacttm - trunc(endacttm/100)*100) - 
      (arrtm - trunc(arrtm/100)*100)]
  }
  tripdata[durdest<0,durdest:=durdest+1440L]
  tripdata[,durdest:=durdest/60]

  setkey(tripdata,hhno,pno,tour,half)
  tripdata <- tripdata[tripdata[,list(maxtripno=max(tseg)),by=list(hhno,pno,tour,half)]]
    
  cats <- c(0,seq(3,27.5,.5))
  tripdata[,arrtimecat:=findInterval(arrtime,cats)]
  tripdata[,deptimecat:=findInterval(deptime,cats)]
  cats <- c(0,seq(0.5,23.5,.5))
  tripdata[,durdestcat:=findInterval(durdest,cats)]
  
  tripdata[,arrflag:= 0]
  tripdata[,depflag:= 0]
  tripdata[,durflag:= 0]
  
  tripdata[half==1 & tseg!=maxtripno,arrflag:= 1]
  tripdata[half==2 & tseg!=1,depflag:= 1]
  tripdata[tseg<maxtripno,durflag:= 1]
  
  return(tripdata)
}

if(prepSurvey)
{
  survperdata <- assignLoad(paste0(surveyperfile,".Rdata"))
  survtripdata <- assignLoad(paste0(surveytripfile,".Rdata"))
  survtripdata <- prep_tripdata(survtripdata,survperdata)
  write_tables(triptodmodelout,survtripdata,triptodmodelfile,"reference")
  
  rm(survperdata,survtripdata)
  gc()
}

if(prepDaySim)
{
  dsperdata <- assignLoad(paste0(dsperfile,".Rdata"))
  dstripdata <- assignLoad(paste0(dstripfile,".Rdata"))
  dstripdata <- prep_tripdata(dstripdata,dsperdata)
  write_tables(triptodmodelout,dstripdata,triptodmodelfile,"new")

  rm(dsperdata,dstripdata)
  gc()
}


print("Trip Time of Day Summary...Finished")