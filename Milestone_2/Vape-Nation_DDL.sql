DROP TABLE IF EXISTS Category;
DROP TABLE IF EXISTS Hours;
DROP TABLE IF EXISTS Friends;
DROP TABLE IF EXISTS Checkin;
DROP TABLE IF EXISTS Tip;
DROP TABLE IF EXISTS Business;
DROP TABLE IF EXISTS Users;

CREATE TABLE Business 
( 
	bid VARCHAR(30),
	name VARCHAR(100) NOT NULL,
	full_address VARCHAR(200) NOT NULL,
	state CHAR(2) NOT NULL,
	city VARCHAR(30) NOT NULL,
	zipcode CHAR(5) NOT NULL,
	latitude FLOAT NOT NULL,
	longitude FLOAT NOT NULL,
	stars FLOAT NOT NULL,
	review_count INTEGER DEFAULT 0,
	openstatus BOOLEAN DEFAULT TRUE,
	numcheckins INTEGER DEFAULT 0,
	PRIMARY KEY (bid)
);

CREATE TABLE Users
( 
	uid VARCHAR(30),
	name VARCHAR(50) NOT NULL,
	average_stars FLOAT DEFAULT 0,
	yelping_since VARCHAR(10) NOT NULL,
	num_fans INTEGER DEFAULT 0,
	cool_votes INTEGER DEFAULT 0,
	funny_votes INTEGER DEFAULT 0,
	useful_votes INTEGER DEFAULT 0,
	PRIMARY KEY (uid)
);

CREATE TABLE Checkin
( 
	bid VARCHAR(30),
	day VARCHAR(10) NOT NULL,
	num_morning INTEGER DEFAULT 0,
	num_afternoon INTEGER DEFAULT 0,
	num_evening INTEGER DEFAULT 0,
	num_night INTEGER DEFAULT 0,
	PRIMARY KEY (bid, day),
	FOREIGN KEY (bid) REFERENCES Business(bid) ON DELETE CASCADE
);

CREATE TABLE Tip
( 
	uid VARCHAR(30),
	bid VARCHAR(30) NOT NULL,
	date CHAR(10) NOT NULL,
	text VARCHAR NOT NULL,
	likes INTEGER DEFAULT 0,
	-- PRIMARY KEY (uid, bid, date),
	FOREIGN KEY (uid) REFERENCES Users(uid) ON DELETE SET NULL,
	FOREIGN KEY (bid) REFERENCES Business(bid) ON DELETE CASCADE
);

CREATE TABLE Friends 
( 
	uid VARCHAR(30),
	fid VARCHAR(30),
	PRIMARY KEY (uid, fid),
	FOREIGN KEY (uid) REFERENCES Users(uid) ON DELETE CASCADE
);

CREATE TABLE Hours 
( 
	bid VARCHAR(30),
	day VARCHAR(10),
	open VARCHAR(5) NOT NULL,
	close VARCHAR(5) NOT NULL,
	PRIMARY KEY (bid, day),
	FOREIGN KEY (bid) REFERENCES Business(bid) ON DELETE CASCADE
);

CREATE TABLE Category 
( 
	bid VARCHAR(30),
	name VARCHAR(100) NOT NULL,
	PRIMARY KEY (bid, name),
	FOREIGN KEY (bid) REFERENCES Business(bid) ON DELETE CASCADE
);