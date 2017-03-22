import json
import psycopg2

# encoding=utf8
import sys
reload(sys)
sys.setdefaultencoding('utf8')

def cleanStr4SQL(s):
    return s.replace("'","''").replace("\n"," ")

def parseBusinessData():
    
    #read the JSON file
    with open('yelp_business.JSON','r') as f:  #Assumes that the data files are available in the current director. If not, you should set the path for the yelp data files.
        line = f.readline()
        count_line = 0
        try:
            #create your database and tables before you run this code.
            #this sample code assumes the database name is "yelpdb"
            conn = psycopg2.connect("dbname= 'Milestone2DB' user='postgres' host='localhost' password='1234'")
        except:
            print('Unable to connect to the postgreSQL database')
            exit(1)
        cur = conn.cursor()

        #read each JSON abject and extract data
        while line:
            data = json.loads(line)
            sql_str = "INSERT INTO Business (bid, name, full_address, state, city, zipcode, latitude, longitude, stars, review_count, numcheckins, openstatus) " \
                      " VALUES ('" + cleanStr4SQL(data['business_id'])+"','"+ \
                                    cleanStr4SQL(data['name'])+"','" + \
                                    cleanStr4SQL(data['full_address'])+"','" + \
                                    cleanStr4SQL(data['state'])+"','" +\
                                    cleanStr4SQL(data['city'])+"','" + \
                                    cleanStr4SQL(data['full_address'])[-5:] + "'," + \
                                    str(data['latitude'])+"," + \
                                    str(data['longitude'])+"," +\
                                    str(data['stars'])+"," + \
                                    str(data['review_count'])+"," +\
                                    str(0) + "," + \
                                    str(data['open']) + ")" #openstatus
            try:
                cur.execute(sql_str)
                count_line += 1
            except:
                print("Insert to businessTABLE failed!")

            # write your own code to process hours
            for day, time in data['hours'].items():
                sql_hour = "INSERT INTO Hours (bid, day, open, close) " \
                " VALUES ('" + cleanStr4SQL(data['business_id'])+ "','" + day + "','" + time['open'] + "','" + time['close'] + "')"
                try:
                    cur.execute(sql_hour)
                except:
                    print("Insert to hourTABLE failed!")

            # write your own code to process category list
            for value in data['categories']:
                sql_cat = "INSERT INTO Category (bid, name) VALUES ('" + cleanStr4SQL(data['business_id'])+ "','" + cleanStr4SQL(value) + "')"
                try:
                    cur.execute(sql_cat)
                except:
                    print("Insert to categoryTABLE failed!")

            conn.commit()
            line = f.readline()

    print("number of tuples inserted: "+str(count_line))
    conn.close()
    f.close()

def parseUserData():

    #read the JSON file
    with open('yelp_user.JSON','r') as f:  #Assumes that the data files are available in the current director. If not, you should set the path for the yelp data files.
        line = f.readline()
        count_line = 0
        try:
            #create your database and tables before you run this code.
            #this sample code assumes the database name is "yelpdb"
            conn = psycopg2.connect("dbname= 'Milestone2DB' user='postgres' host='localhost' password='1234'")
        except:
            print('Unable to connect to the postgreSQL database')
            exit(1)
        cur = conn.cursor()

        while line:
            data = json.loads(line)
            sql_str = "INSERT INTO Users (uid, name, average_stars, yelping_since, num_fans, cool_votes, funny_votes, useful_votes) " \
                      " VALUES ('" + cleanStr4SQL(data['user_id'])+"','"+ \
                                    cleanStr4SQL(data['name'])+"'," + \
                                    str(data['average_stars'])+",'" +\
                                    cleanStr4SQL(data['yelping_since'])+"'," + \
                                    str(data['fans']) + "," + \
                                    str(data['votes']['cool'])+"," + \
                                    str(data['votes']['funny'])+"," + \
                                    str(data['votes']['useful'])+")"
            try:
                cur.execute(sql_str)
                count_line += 1
            except:
                print("Insert to userTABLE failed!")

            # add friends
            for fid in data['friends']:
                sql_friend = "INSERT INTO Friends (uid, fid) VALUES ('" + cleanStr4SQL(data['user_id'])+ "','" + cleanStr4SQL(fid) + "')"
                try:
                    cur.execute(sql_friend)
                except:
                    print("Insert to friendTABLE failed!")
                    print(sql_friend)

            conn.commit()
            line = f.readline()

    print("number of tuples inserted: "+str(count_line))
    conn.close()
    f.close()

def parseCheckinData():
    
    #read the JSON file
    with open('yelp_checkin.JSON','r') as f:  #Assumes that the data files are available in the current director. If not, you should set the path for the yelp data files.
        line = f.readline()
        count_line = 0
        try:
            #create your database and tables before you run this code.
            #this sample code assumes the database name is "yelpdb"
            conn = psycopg2.connect("dbname= 'Milestone2DB' user='postgres' host='localhost' password='1234'")
        except:
            print('Unable to connect to the postgreSQL database')
            exit(1)
        cur = conn.cursor()

        while line:
            data = json.loads(line)

            dayCheckin = {}
            for item, value in data['checkin_info'].items():
                
                day_time = item.split('-')
                time = int(day_time[0])
                
                day = int(day_time[1])
                if day == 0:
                    daystr = 'Sunday'
                elif day == 1:
                    daystr = 'Monday'
                elif day == 2:
                    daystr = 'Tuesday'
                elif day == 3:
                    daystr = 'Wednesday'
                elif day == 4:
                    daystr = 'Thursday'
                elif day == 5:
                    daystr = 'Friday'
                else:
                    daystr = 'Saturday'

                value = int(value) # num of checkins

                dayCheckin[daystr] = dayCheckin.get(daystr, [0,0,0,0])

                if (time in range (6, 12)):
                    dayCheckin[daystr][0] += value
                elif (time in range (12, 17)):
                    dayCheckin[daystr][1] += value
                elif (time in range (17, 23)):
                    dayCheckin[daystr][2] += value
                else:# (time in range (23, 6)):
                    dayCheckin[daystr][3] += value

            for day, value in dayCheckin.items():            
                sql_str = "INSERT INTO Checkin (bid, day, num_morning, num_afternoon, num_evening, num_night) " \
                      "VALUES ('" + data['business_id'] + "','" + day + "'," + str(value[0]) + "," + str(value[1]) + "," + str(value[2]) + "," + str(value[3]) + ")"
                try:
                    cur.execute(sql_str)
                    count_line += 1
                except:
                    print("Insert to checkinTABLE failed!")

            conn.commit()
            line = f.readline()

    print("number of tuples inserted: "+str(count_line))
    conn.close()
    f.close()


def parseTipsData():
    
    #read the JSON file
    with open('yelp_tip.JSON','r') as f:  #Assumes that the data files are available in the current director. If not, you should set the path for the yelp data files.
        line = f.readline()
        count_line = 0
        try:
            #create your database and tables before you run this code.
            #this sample code assumes the database name is "yelpdb"
            conn = psycopg2.connect("dbname= 'Milestone2DB' user='postgres' host='localhost' password='1234'")
        except:
            print('Unable to connect to the postgreSQL database')
            exit(1)
        cur = conn.cursor()

        while line:
            data = json.loads(line)
            sql_str = "INSERT INTO Tip (uid, bid, date, text, likes) " \
                      " VALUES ('" + cleanStr4SQL(data['user_id'])+"','" + \
                                    cleanStr4SQL(data['business_id'])+"','" + \
                                    cleanStr4SQL(data['date'])+"','" + \
                                    cleanStr4SQL(data['text'])+"',"+ \
                                    str(data['likes'])+")"
                                    
            try:
                cur.execute(sql_str)
                count_line += 1
            except:
                s = 0
                #print("Insert to tipTABLE failed!")
            
            conn.commit()
            line = f.readline()

    print("number of tuples inserted: "+str(count_line))
    conn.close()
    f.close()

parseBusinessData()
parseUserData()
parseCheckinData()
parseTipsData()
