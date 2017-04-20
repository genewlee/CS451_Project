-- User
SELECT * FROM Users where uid='';

-- Friends
SELECT F.name, Business.name, Business.city, Tip.text, Tip.Date 
FROM(
	SELECT Users.uid, Users.name
	FROM Friends, Users
	WHERE Users.uid = Friends.fid
	AND Friends.uid = ''
	) AS F, Tip, Business
		WHERE F.uid = Tip.uid AND Tip.bid = Business.bid
ORDER BY Tip.Date DESC; 

-- Tips from friends
SELECT Users.name, Users.average_stars, Users.yelping_since, Users.uid
FROM Friends, Users
WHERE Users.uid = Friends.fid
AND Friends.uid = '{0}'
ORDER BY Users.name

-- Remove friend
DELETE FROM Friends WHERE uid='' AND fid='';

-- Rate friend
UPDATE Users SET average_stars=  WHERE uid = '';

-- Tips from selected friend
SELECT F.name, Business.name, Business.city, Tip.text, Tip.Date 
FROM(
	SELECT uid, name
	FROM Users
	WHERE uid = ''
	) AS F, Tip, Business
WHERE F.uid = Tip.uid AND Tip.bid = Business.bid
ORDER BY Tip.Date DESC; 

-- States
SELECT DISTINCT state FROM business ORDER BY state;

-- Cities in state
SELECT DISTINCT city FROM business WHERE state='' ORDER BY city

-- Zip codes in state
SELECT DISTINCT zipcode FROM business WHERE state='' ORDER BY zipcode

-- Zip codes in state and city
SELECT DISTINCT zipcode FROM business WHERE state='' AND city='' ORDER BY zipcode

-- Categories
SELECT DISTINCT(category.name)
FROM business, category
WHERE business.bid = category.bid 
AND state ='' AND city = ''  
ORDER BY category.name;

-- Categories by state, city, and zip
SELECT DISTINCT(category.name)
FROM business, category
WHERE business.bid = category.bid 
AND state ='' AND city = '' AND zipcode = '' ORDER BY category.name;

-- Categories by state and zip
SELECT DISTINCT(category.name)
FROM business, category
WHERE business.bid = category.bid 
AND state ='' AND zipcode = '' ORDER BY category.name;

-- Search for businesses
SELECT DISTINCT(B.bid), B.name, B.full_address, B.city, B.state, B.zipcode, B.review_count, B.numcheckins, H.open, H.close
FROM business as B, category as C, hours H
WHERE B.bid = C.bid AND B.bid = H.bid
AND state='{0}' AND city='{0}' AND zipcode='{0}'
AND (C.name='{0}') -- more categories if multiple with ORs
-- if hour and/or days is selected
AND H.day='{0}' 
AND H.open<='{1}' AND '{2}'<H.close
AND H.open<='{3}' AND '{4}'<=H.close
-- if 24 hour open are selected
AND H.day='{0}' AND H.open='00:00' AND '00:00'=H.close

-- checkin
UPDATE checkin 
SET  -- add 1 to checkin time
WHERE bid ='' and day='';

-- insert tip
INSERT INTO tip(uid,bid,text,likes,date) VALUES ('','', '', 0,'');

-- -- CHARTS

-- num checkins
SELECT day, num_morning+num_afternoon+num_evening+num_night
FROM checkin
WHERE bid='' 

-- business per cat
SELECT c.name, COUNT(c.bid)
FROM business b, category c
WHERE c.bid=b.bid
AND state='' AND city='' AND zipcode=''
GROUP BY c.name ORDER BY c.name;

-- avg stars
SELECT c.name, SUM(b.stars)/COUNT(b.stars) as avgStars
FROM business b, category c
WHERE c.bid=b.bid
AND state='{0}' AND city='' AND zipcode=''
GROUP BY c.name ORDER BY c.name;