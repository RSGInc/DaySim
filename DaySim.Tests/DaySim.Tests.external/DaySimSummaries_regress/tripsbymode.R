
#Summarize trips by mode and pathtype test
#Ben Stabler, ben.stabler@rsginc.com, 12/8/15

#read trips
trips = read.csv("outputs/_trip.tsv", sep="\t")

#tabulate trips by mode and pathtype
outtable = as.data.frame(table(trips$mode,trips$pathtype))

#add column headers
colnames(outtable) = c("mode","pathtype","count")

#write resulting CSV table
write.csv(outtable,"tripsbymode.csv",row.names=F)
