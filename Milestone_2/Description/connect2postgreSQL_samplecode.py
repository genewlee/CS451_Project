import json
import psycopg2
# to install psycopg2 run the command:   pip install psycopg2
#  visit http://initd.org/psycopg/download/ for more information


def cleanStr4SQL(s):
    return s.replace("'","`").replace("\n"," ")

def parseBusinessData():
    #read the JSON file
    with open('yelp_business.JSON','r') as f:  #Assumes that the data files are available in the current director. If not, you should set the path for the yelp data files.
        line = f.readline()
        count_line = 0
        try:
            #create your database and tables before you run this code.
            #this sample code assumes the database name is "yelpdb"
            conn = psycopg2.connect("dbname= 'yelpdb' user='postgres' host='localhost' password='mustafa'")
        except:
            print('Unable to connect to the postgreSQL database')
            exit(1)
        cur = conn.cursor()

        #read each JSON abject and extract data
        while line:
            data = json.loads(line)
            sql_str = "INSERT INTO businessTable (business_id, name,full_address, state,city, zipcode, latitude, longitude, stars, reviewcount, numcheckins, openstatus) " \
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
                                    str(data['open'])+")" #openstatus
            try:
                cur.execute(sql_str)
                count_line += 1
            except:
                print("Insert to businessTABLE failed!")
            conn.commit()
            line = f.readline()

    # write your own code to process hours
    # write your own code to process category list
    print("number of tuples inserted: "+str(count_line))
    conn.close()
    f.close()

def parseUserData():
    #write code to parse yelp_user.JSON
    pass

def parseCheckinData():
    #write code to parse yelp_checkin.JSON
    pass


def parseTipsData():
    #write code to parse yelp_tip.JSON
    pass

parseBusinessData()
parseUserData()
parseCheckinData()
parseTipsData()
