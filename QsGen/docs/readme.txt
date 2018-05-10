file = *nlc-section bands-section;
nlc-section = nlc-header popular-section quick-section
nlc-header = ".nlc" <nlc> *tvm-name
popularsection = popular-header *popular-line
popular-header = ".pops" location-list
popular-line = location-list
location-list = *(<crs> | <nlc>)
quick-section=quicks-header *quick-line
quicks-header=".quicks"
quick-line=orientation destination <route> <tickettype> <days> timeband *kvpair
orientation="left"|"right"
destination= <nlc>|<crs>
timeband = tb equals <tbname>



