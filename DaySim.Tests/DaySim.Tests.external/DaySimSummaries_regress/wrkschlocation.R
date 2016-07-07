###This script generates summaries for DaySim Usual Work and School Locations
###Distributions of home-work/school distances and times by persontype are produced

print("Work/School Location Summary...Started")

prep_wrkschloc <- function(perdata,hhdata)
{
  perdata <- merge(perdata,hhdata,by="hhno",all.x=T)
  perdata[,wrkr:=ifelse(pwtyp>0 & pwtaz!=0,1,0)]
  perdata[,no_wloc:=ifelse(pwtyp>0 & pwtaz==0,1,0)]
  perdata[,outhmwrkr:=ifelse(pwtaz>0 & hhparcel!=pwpcl,1,0)]
  perdata[,wrkrtyp:=c(1,2,3,3,3,3,3,3)[pptyp]]
  perdata[,wrkdistcat:=findInterval(pwaudist,0:89)]
  perdata[,wrktimecat:=findInterval(pwautime,0:89)]
  perdata[pwtaz<0,wrkdistcat:=91]
  perdata[pwtaz<0,wrktimecat:=91]
  perdata[,stud:=ifelse(pptyp %in% c(5:7) & pstaz!=0,1,0)] 
  perdata[,no_sloc:=ifelse(pptyp %in% c(5:7) & pstaz==0,1,0)]
  perdata[,outhmstud:=ifelse(pstaz>0 & hhparcel!=pspcl,1,0)]
  perdata[,stutyp:=c(4,4,4,4,3,2,1,4)[pptyp]]
  perdata[,schdistcat:=findInterval(psaudist,0:89)]
  perdata[,schtimecat:=findInterval(psautime,0:89)]
  perdata[pstaz<0,schdistcat:=91]
  perdata[pstaz<0,schtimecat:=91]
  perdata[,hhcounty:=countycorr$DISTRICT[match(hhtaz,countycorr$TAZ)]]
  perdata[,pwcounty:=countycorr$DISTRICT[match(pwtaz,countycorr$TAZ)]]
  perdata[,pscounty:=countycorr$DISTRICT[match(pstaz,countycorr$TAZ)]]
  perdata[pwtaz<0,pwcounty:=8]
  perdata[pstaz<0,pscounty:=8]
  perdata[,wfh:=ifelse(wrkr==1 & hhparcel==pwpcl,1,0)]
  perdata[,sfh:=ifelse(stud==1 & hhparcel==pspcl,1,0)]
  perdata[pwautime<0,pwautime:=NA]
  perdata[pwaudist<0,pwaudist:=NA]
  perdata[psautime<0,psautime:=NA]
  perdata[psaudist<0,psaudist:=NA]
  return(perdata)
}

if(prepSurvey)
{
  survperdata <- assignLoad(paste0(surveyperfile,".Rdata"))
  survhhdata <- assignLoad(paste0(surveyhhfile,".Rdata"))
  survperdata <- prep_wrkschloc(survperdata,survhhdata)
  write_tables(wrklocmodelout,survperdata,wrklocmodelfile,"reference")
  write_tables(schlocmodelout,survperdata,schlocmodelfile,"reference")
  rm(survperdata,survhhdata)
  gc()
}

if(prepDaySim)
{
  dsperdata <- assignLoad(paste0(dsperfile,".Rdata"))
  dshhdata <- assignLoad(paste0(dshhfile,".Rdata"))
  dsperdata <- prep_wrkschloc(dsperdata,dshhdata)
  write_tables(wrklocmodelout,dsperdata,wrklocmodelfile,"new")
  write_tables(schlocmodelout,dsperdata,schlocmodelfile,"new")
  rm(dsperdata,dshhdata)
  gc()
}

print("Work/School Location Summary...Finished")
