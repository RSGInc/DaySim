###This script generates summaries for DaySim vehicle availability models
###Distributions of vehicle ownership by drivers, income and county are produced

print("Vehicle Availabililty Summary...Started")

prep_vehavail <- function(hhdata,perdata)
{
  hhdata[,hhvehcat:=ifelse(hhvehs>4,4,hhvehs)]
  perdata[,hh16cat:=ifelse(pagey>=16,1,0)]
  aggper <- perdata[,list(hh16cat=sum(hh16cat)),by=hhno]
  hhdata <- merge(hhdata,aggper,by="hhno")
  hhdata[hh16cat>4,hh16cat:=4]
  hhdata[,inccat:=1+findInterval(hhincome,c(15000,50000,75000))]
  hhdata[,hhcounty:=countycorr$DISTRICT[match(hhtaz,countycorr$TAZ)]]
  return(hhdata)
}

if(prepSurvey)
{
  survperdata <- assignLoad(paste0(surveyperfile,".Rdata"))
  survhhdata <- assignLoad(paste0(surveyhhfile,".Rdata"))
  survhhdata <- prep_vehavail(survhhdata,survperdata)
  write_tables(vehavmodelout,survhhdata,vehavmodelfile,"reference")
  rm(survhhdata,survperdata)
  gc()
}

if(prepDaySim)
{
  dsperdata <- assignLoad(paste0(dsperfile,".Rdata"))
  dshhdata <- assignLoad(paste0(dshhfile,".Rdata"))
  dshhdata <- prep_vehavail(dshhdata,dsperdata)
  write_tables(vehavmodelout,dshhdata,vehavmodelfile,"new")
  rm(dshhdata,dsperdata)
  gc()
}

print("Vehicle Availabililty Summary...Finished")