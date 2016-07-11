###TODO review functions for speed/memory use
###TODO consider changes to convert to data.table
###TODO any overlap with base or package functions?
###TODO try to simplify/break up long functions

progressStart <- function(pbtitle,steps){
  
  #Record start time for the step and start data frame for recording times
  modstarttime <<- proc.time()
  runtimes <<- data.frame(steps=1:steps,stepnames=NA,stepruntimes=as.numeric(0),cumruntimes=as.numeric(0))
  
  #Initiate progress
  pb_globalsummary <<- winProgressBar(title = pbtitle, min = 0, max = steps, width = 600)
  currentstep <<- 0
  pbtitle <<- pbtitle
}

progressNextStep <- function(stepname){
  
  #Record run time if we are finishing a step
  if(currentstep > 0){
    runtimes$cumruntimes[runtimes$steps == currentstep] <<- (proc.time()-modstarttime)[3]
  }
  
  #Increment step
  currentstep <<- currentstep + 1
  message <- paste(pbtitle,stepname,sep=": ")
  print(message)
  setWinProgressBar(pb_globalsummary, currentstep, title=message)
  runtimes$stepnames[runtimes$steps == currentstep] <<- stepname
  
}

progressEnd <- function(outdir){
  
  close(pb_globalsummary)
  
  #Record run time for last step, complete run times and print/write
  if(currentstep > 0){
    runtimes$cumruntimes[runtimes$steps == currentstep] <<- (proc.time()-modstarttime)[3]
    runtimes$stepruntimes <<- c(runtimes$cumruntimes[1],diff(runtimes$cumruntimes)) 
    print(runtimes)
    write.csv(runtimes,paste(outdir,"runtimes.csv",sep="/"),row.names=FALSE)  
  }
}

#load an RData object to an object name
assignLoad <- function(filename){
  load(filename)
  get(ls()[ls() != "filename"])
}

#read a file, save to Rdata, and remove from workspace
readSaveRdata <- function(filename,objname){
  assign(objname,fread(filename))
  save(list=objname,file=paste0(filename,".Rdata"))
  rm(list=objname)
}

##Smoothing function
smooth_fun <- function(vals)
{
  tempvals <- vals
  for(j in 1:10)
  {
    tempvals <- filter(tempvals,c(.25,.5,.25))
    tempvals[1] <- vals[1]
    tempvals[length(tempvals)] <- vals[length(vals)]
  }
  tempvals
}

#Output function
#output(out,tabs$outnum[i],wb,tabs$outsheet[i],tabs$cell_loc[i],xvals,yvals)
output <- function(out,outnum,wb,outsheet,celloc,xvals=0,yvals=0,transpose=0)
{ 
  outsheet <- as.character(outsheet)
  celloc <- as.character(celloc)
  if(length(xvals)>1 & length(yvals)>1)
  {
    base <- expand.grid(Var1=xvals,Var2=yvals)
    out <- merge(base,out,all.x=T)
		out[is.na(out)] <- 0
		out <- cast(out, Var1 ~ Var2, value = "Freq")
    out <- out[,-1]
	}
	if(length(xvals)>1 & length(yvals)<=1)
	{
		base <- data.frame(Var1=xvals)
		out <- merge(base,out,by="Var1",all.x=T)
		out[is.na(out)] <- 0
    out <- data.frame(val=as.numeric(out[,2]))
    if(transpose!=0)
      out <- data.frame(t(out))
	}
  regname <- paste("out",outnum,sep="")
  createName(wb, name = regname, formula = paste(outsheet,"!",celloc,sep=""),overwrite=TRUE)
	writeNamedRegion(wb, out, name = regname,header=F)
}

filterdt <- function(dt,var,sign,val)
{
  var <- as.character(var)
  sign <- as.character(sign)
  val <- as.numeric(val)
  if(sign == "=" | sign == "==")
    dt <- dt[dt[[var]] == val]
  else if(sign == "!=")
    dt <- dt[dt[[var]] != val]
  else if(sign == ">")
    dt <- dt[dt[[var]] > val]
  else if(sign == "<")
    dt <- dt[dt[[var]] < val]
  else if(sign == ">=")
    dt <- dt[dt[[var]] >= val]
  else if(sign == "<=")
    dt <- dt[dt[[var]] <= val]
  return(dt)
}

my_loadworkbook <- function(outfilename) {
  workbook_name <- paste(outputsDir,outfilename,sep="/")
  
  #look for spreadsheet in output directory. If not there copy from excel template folder
  if (!file.exists(workbook_name)) {
    original_excel_template = paste('excel_report_files',outfilename,sep="/")
    if (!dir.exists(outputsDir)) {
      dir.create(outputsDir)
    }
    file.copy(original_excel_template, workbook_name)
    if (!file.exists(original_excel_template)) {
      stop(paste0("Workbook file '",original_excel_template,"' does not exist!"))
    }
  }
  wb = loadWorkbook(workbook_name)
}

write_tables <- function(outfilename,datafile,templatefilename,outtype)
{
    templatefile <- read.csv(templatefilename)
    wb = my_loadworkbook(outfilename)
    setStyleAction(wb,XLC$"STYLE_ACTION.NONE")
    tabulate_summaries(datafile,templatefile,outtype,wb) #TODO update function to work with data.tables
    saveWorkbook(wb)
}


tab_crosstab <- function(df,tabrow,wb)
{
  dim <- tabrow$dim
  wtvar <- as.character(tabrow$weights)
  var1 <- as.character(tabrow$Var1)
  xvals <- c(tabrow$xvalsmin:tabrow$xvalsmax)
  if(dim==1){
    if(wtvar %in% c("",NA)){
      out <- data.frame(table(df[[var1]]))
    } else{
      out <- data.frame(wtd.table(df[[var1]],weights=df[[wtvar]]))
    }
    names(out) <- c("Var1","values")
    out$Var1 <- as.integer(as.character(out$Var1))
    out <- merge(data.frame(Var1=xvals),out,by="Var1",all.x=T)
    out[is.na(out)] <- 0
    out <- out[order(out$Var1),]
    if("smooth" %in% names(tabrow))
    {
      if(!tabrow$smooth %in% c("",NA))
      {
        out$values <- smooth_fun(out$values)
        #out$values <- (loess(values~Var1,out))$fitted
      }
    }
    output(out,tabrow$outnum,wb,tabrow$outsheet,tabrow$cell_loc,xvals) 
  } else if(dim==2){
    var2 <- as.character(tabrow$Var2)
    yvals <- c(tabrow$yvalsmin:tabrow$yvalsmax)
    if(wtvar %in% c("",NA)){
      out <- data.frame(table(df[[var1]], df[[var2]]))
    } else{
      #out <- data.frame((crosstab(df[[var1]], df[[var2]], weight = df[[wtvar]], plot=F))$t) #replace with DT funciton
      out <- df[,list(sum(get(wtvar),na.rm=TRUE)),by=c(var1,var2)]
    }
    setnames(out,c("Var1","Var2","Freq"))
    out$Var1 <- as.integer(as.character(out$Var1))
    out$Var2 <- as.integer(as.character(out$Var2))
    out <- merge(expand.grid(Var1=xvals,Var2=yvals),out,all.x=T)
    out[is.na(out)] <- 0
    out <- out[order(out$Var1,out$Var2),]
    if("smooth" %in% names(tabrow))
    {
      if(!tabrow$smooth %in% c("",NA))
      {
        dim2 <- data.frame(table(out$Var2))$Var1
        for(tempvar in dim2)
        {
          out$Freq[out$Var2==tempvar] <- smooth_fun(out$Freq[out$Var2==tempvar])
          #out$Freq[out$Var2==tempvar] <- (loess(Freq~Var1,out[out$Var2==tempvar,]))$fitted
        }   
      }
    }
    output(out,tabrow$outnum,wb,tabrow$outsheet,tabrow$cell_loc,xvals,yvals)
  }
}

tab_aggregate <- function(df,tabrow,wb)
{
  aggfun <- as.character(tabrow$aggfun)
  var1 <- as.character(tabrow$Var1)
  var2 <- as.character(tabrow$Var2)
  wtvar <- as.character(tabrow$weights)
  if(aggfun=="mean")
  {
    if(wtvar %in% c("",NA)){
      if(!var2 %in% c("",NA))
        out <- aggregate(df[[var1]],by=list(df[[var2]]),mean,na.rm=T)
      totmean <- mean(df[[var1]],na.rm=T)
    } else{
      if(!var2 %in% c("",NA))
        out <- ddply(df,var2,function(x){wtd.mean(x[[var1]],x[[wtvar]],na.rm=T)})
      totmean <- wtd.mean(df[[var1]],weights=df[[wtvar]],na.rm=T)
    }         
    if(!var2 %in% c("",NA))
    {
      names(out) <- c("Var1","values")
      yvals <- c(tabrow$yvalsmin:(tabrow$yvalsmax+1))
      out[nrow(out)+1,] <- c(max(yvals),totmean)
    } else{
      out <- data.frame(values=totmean)
      yvals <- 1
    } 
    output(out,tabrow$outnum,wb,tabrow$outsheet,tabrow$cell_loc,yvals)
  } else{
    if(wtvar %in% c("",NA)){
      #out <- aggregate(df[[var1]],by=list(df[[var2]]),sum,na.rm=T)
      out<-df[,list(sum(get(var1),na.rm=TRUE)),by=var2]
    } else{
      #out <- aggregate(df[[var1]]*df[[wtvar]],by=list(df[[var2]]),sum,na.rm=T)
      out<-df[,list(sum(get(var1)*get(wtvar),na.rm=TRUE)),by=var2]
    } 
    setnames(out,c("Var1","values")) 
    yvals <- c(tabrow$yvalsmin:tabrow$yvalsmax)
    if(!tabrow$transpose %in% c("",NA))
      output(out,tabrow$outnum,wb,tabrow$outsheet,tabrow$cell_loc,yvals,transpose=1) else
        output(out,tabrow$outnum,wb,tabrow$outsheet,tabrow$cell_loc,yvals)        
  }
}
##TODO this is very long and difficult to understand
##TODO suggest simplyfying/breaking up to simpler functions
tabulate_summaries <- function(fulldf,tabs,datasource,wb)
{
  tabs <- tabs[tabs$data==datasource,]
  for(i in 1:nrow(tabs))
  {
    if(!tabs$subsetvar[i] %in% c("",NA))
      df <- filterdt(fulldf,tabs$subsetvar[i],tabs$subsetsign[i],tabs$subsetval[i]) else
        df <- fulldf
    if(nrow(df)>0)
    {
      if(as.character(tabs$type[i]) %in% c("crosstab"))
      {
        tab_crosstab(df,tabs[i,],wb)     
      } else{
        tab_aggregate(df,tabs[i,],wb) 
      }     
    }
  }
}