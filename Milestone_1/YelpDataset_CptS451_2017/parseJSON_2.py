import json

# encoding=utf8
import sys
reload(sys)
sys.setdefaultencoding('utf8')

def cleanStr4SQL(s):
    return s.replace("'","''").replace("\n"," ")

def parseBusinessData():
    #read the JSON file
    with open('yelp_business.JSON','r') as f:  #Assumes that the data files are available in the current director. If not, you should set the path for the yelp data files.  
        outfile =  open('business.txt', 'w')
        line = f.readline()
        count_line = 0
        #read each JSON abject and extract data
        outfile.write('# business_id, name, full_address, zipcode, state, city, lat, long, stars, openstatus, category_list, hours\n')
        while line:
            data = json.loads(line)
            outfile.write(cleanStr4SQL(data['business_id'])+'\t') #business id
            outfile.write(cleanStr4SQL(data['name'])+'\t') #name
            
            full_address = cleanStr4SQL(data['full_address'])
            outfile.write(full_address +'\t') #full_address
            zipcode = full_address[-6:]
            outfile.write(zipcode + '\t')
            
            outfile.write(cleanStr4SQL(data['state'])+'\t') #state
            outfile.write(cleanStr4SQL(data['city'])+'\t') #city
            outfile.write(str(data['latitude'])+'\t') #latitude
            outfile.write(str(data['longitude'])+'\t') #longitude
            outfile.write(str(data['stars'])+'\t') #stars
            #outfile.write(str(data['review_count'])+'\t') #reviewcount
            outfile.write(str(data['open'])+'\t') #openstatus
            #outfile.write(str([item for item in  data['categories']])+'\t') #category list

            outfile.write('[')
            dayHours = {}
            for day, value in data['hours'].items():
                open_time = value['open']
                close_time = value['close']
                dayHours[day] = dayHours.get(day, ['', ''])
                dayHours[day][0] = open_time
                dayHours[day][1] = close_time
            for key, value in dayHours.items():
                outfile.write('({}, {}, {}),'.format(key, value[0], value[1]))
            outfile.write(']')

            outfile.write('\n');

            line = f.readline()
            count_line +=1
    print(count_line)
    outfile.close()
    f.close()

def parseUserData():
    #read the JSON file
    with open('yelp_user.JSON','r') as f:  #Assumes that the data files are available in the current director. If not, you should set the path for the yelp data files.  
        outfile =  open('user.txt', 'w')
        line = f.readline()
        count_line = 0
        #read each JSON abject and extract data
        outfile.write('# user_id, name, type, yelping_since, stars, votes_funny, votes_useful, votes_cool, friends_list, fans\n')
        while line:
            data = json.loads(line)
            outfile.write(cleanStr4SQL(data['user_id'])+'\t') #user id
            outfile.write(cleanStr4SQL(data['name'])+'\t') #name
            outfile.write(cleanStr4SQL(data['type'])+'\t') #type
            outfile.write(cleanStr4SQL(data['yelping_since'])+'\t') #since date
            outfile.write(str(data['average_stars'])+'\t') #stars

            outfile.write(str(data['votes']['funny']) +'\t') #votes funny
            outfile.write(str(data['votes']['useful']) +'\t') #votes useful
            outfile.write(str(data['votes']['cool']) +'\t') #votes cool

            outfile.write(str([item for item in data['friends']])+'\t') #friends list
            outfile.write(str(data['fans'])+'\t') #fans
            
            outfile.write('\n');

            line = f.readline()
            count_line +=1
    print(count_line)
    outfile.close()
    f.close()

def parseCheckinData():
    #code to parse yelp_checkin.JSON
    #read the JSON file
    with open('yelp_checkin.JSON','r') as f:  #Assumes that the data files are available in the current director. If not, you should set the path for the yelp data files.  
        outfile =  open('checkin.txt', 'w')
        line = f.readline()
        count_line = 0
        #read each JSON abject and extract data
        outfile.write('# business_id, type, [checkin_day [morning, afternoon, evening, night]]\n')
        while line:
            data = json.loads(line)
            outfile.write(cleanStr4SQL(data['business_id'])+'\t') #business id
            outfile.write(cleanStr4SQL(data['type'])+'\t') #type

            dayCheckin = {}
            for item, value in data['checkin_info'].items():
                day_time = item.split('-')
                time = int(day_time[0])
                day = day_time[1]
                value = int(value)
                dayCheckin[day] = dayCheckin.get(day, [0,0,0,0])
                if (time in range (6, 12)):
                    dayCheckin[day][0] += value
                elif (time in range (12, 17)):
                    dayCheckin[day][1] += value
                elif (time in range (17, 23)):
                    dayCheckin[day][2] += value
                else:# (time in range (23, 6)):
                    dayCheckin[day][3] += value
            # TODO: outfile.write(str([])) # write code to process hour of day
            for key, value in dayCheckin.items():
                outfile.write('{} [{} {} {} {}] '.format(key, value[0], value[1], value[2], value[3]))

            outfile.write('\n');

            line = f.readline()
            count_line +=1
    print(count_line)
    outfile.close()
    f.close()

def parseTipsData():
    #code to parse yelp_tip.JSON
    #read the JSON file
    with open('yelp_tip.JSON','r') as f:  #Assumes that the data files are available in the current director. If not, you should set the path for the yelp data files.  
        outfile =  open('tip.txt', 'w')
        line = f.readline()
        count_line = 0
        #read each JSON abject and extract data
        outfile.write('# user_id, business_id, text, likes, date, type\n')
        while line:
            data = json.loads(line)
            outfile.write(cleanStr4SQL(data['user_id'])+'\t') #user id
            outfile.write(cleanStr4SQL(data['business_id'])+'\t') #business id
            outfile.write(cleanStr4SQL(data['text'])+'\t') #text
            
            outfile.write(str(data['likes'])+'\t') #likes
            outfile.write(cleanStr4SQL(data['date'])+'\t') #date
            outfile.write(cleanStr4SQL(data['type'])+'\t') #type
        
            outfile.write('\n');
            
            line = f.readline()
            count_line +=1
    print(count_line)
    outfile.close()
    f.close()

parseBusinessData()
#parseUserData()
#parseCheckinData()
#parseTipsData()
