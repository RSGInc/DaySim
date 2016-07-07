###This script generates Day Patterns from DaySim run outputs

print("Tour Destination Summary...Started")

prep_perdata <- function(perdata,hhdata)
{
  hhdata[,hhcounty:=countycorr$DISTRICT[match(hhtaz,countycorr$TAZ)]]
  perdata <- merge(perdata,hhdata,by="hhno",all.x=T)
  return(perdata)
}

merge_skims <- function(tourdata)
{
  #Map the timeperiods for the two tour halves
  nm2 <- names(tourdata)
  tourdata[,h1timp:=findInterval(tardest,c(360,540,930,1110))]
  tourdata[h1timp==0,h1timp:=4]
  tourdata[,h2timp:=findInterval(tlvdest,c(360,540,930,1110))]
  tourdata[h2timp==0,h2timp:=4]
  skimsfiles <- c(amskimfile,mdskimfile,pmskimfile,evskimfile)
  for(k in 1:4)
  {
    #read matrices
    timemat <- t( h5read(skimsfiles[k], "Skims/svtl2t") )
    distmat <- t( h5read(skimsfiles[k], "Skims/svtl2d") )
    skims <- data.frame(rep(countycorr$TAZ, length(countycorr$TAZ)), 
      rep(countycorr$TAZ, each=length(countycorr$TAZ)), 
      as.vector(timemat), as.vector(distmat)
      )
    
    setnames(skims,c("totaz","tdtaz","H1TIM","H1DIS"))
    skims <- skims[,c("totaz","tdtaz","H1TIM","H1DIS")]
    tourdata[h1timp==k,H1TIM:=skims$H1TIM[match(paste(totaz,tdtaz,sep="-"),paste(skims$totaz,skims$tdtaz,sep="-"))]] 
    tourdata[h1timp==k,H1DIS:=skims$H1DIS[match(paste(totaz,tdtaz,sep="-"),paste(skims$totaz,skims$tdtaz,sep="-"))]]
    setnames(skims,c("totaz","tdtaz","H2TIM","H2DIS"))
    tourdata[h2timp==k,H2TIM:=skims$H2TIM[match(paste(totaz,tdtaz,sep="-"),paste(skims$totaz,skims$tdtaz,sep="-"))]] 
    tourdata[h2timp==k,H2DIS:=skims$H2DIS[match(paste(totaz,tdtaz,sep="-"),paste(skims$totaz,skims$tdtaz,sep="-"))]]
    rm(skims)         
  }
  tourdata[,tautotime:= H1TIM + H2TIM]
  tourdata[,tautodist:= H1DIS + H2DIS]
  return(tourdata[,c(nm2),with=F])
}

prep_tourdata <- function(tourdata,perdata)
{
  tourdata <- merge(tourdata,perdata,by=c("hhno","pno"),all.x=T)
  if(excludeChildren5)
    tourdata <- tourdata[pptyp<8,]
  tourdata[pdpurp==8,pdpurp:=7]
  tourdata[pdpurp==9,pdpurp:=4]
  tourdata[,pdpurp2:=ifelse(parent == 0,pdpurp,8)]
  tourdata[,ocounty:=countycorr$DISTRICT[match(totaz,countycorr$TAZ)]]
  tourdata[,dcounty:=countycorr$DISTRICT[match(tdtaz,countycorr$TAZ)]]
  if(sum(tourdata$tautodist,na.rm=T)==0)
  {
    tourdata <- merge_skims(tourdata)
  }
  
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