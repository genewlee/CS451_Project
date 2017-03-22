-- Creates triggers to enforce the following constraints in database:  

-- Whenever a new tip is provided for a business, the 'review_count' value for that business should
-- be automatically updated. 

DROP TRIGGER IF EXISTS update_review_count ON Tip;

CREATE OR REPLACE FUNCTION aft_insert_tip()
RETURNS TRIGGER AS $$
BEGIN
	UPDATE Business
	SET review_count = review_count + 1
	WHERE NEW.bid = Business.bid AND Business.openstatus = true;
	RETURN NEW;
END; $$ LANGUAGE 'plpgsql';

CREATE TRIGGER update_review_count
AFTER INSERT ON Tip
FOR EACH ROW 
EXECUTE PROCEDURE aft_insert_tip();

-- When a customer checks-in a business, the 'numcheckins' value for that business 
-- should be automatically updated. 

DROP TRIGGER IF EXISTS update_numcheckins ON Checkin;

CREATE OR REPLACE FUNCTION aft_check_in()
RETURNS TRIGGER AS $$
BEGIN
	UPDATE Business
	SET numcheckins = numcheckins + 1
	WHERE NEW.bid = Business.bid;
	RETURN NEW;
END; $$ LANGUAGE 'plpgsql';

CREATE TRIGGER update_numcheckins
AFTER INSERT OR UPDATE ON Checkin
FOR EACH ROW 
EXECUTE PROCEDURE aft_check_in();

-- Customers can write tips for “open” (i.e., active) businesses only.

DROP TRIGGER IF EXISTS add_tip_open_status ON Tip;

CREATE OR REPLACE FUNCTION bef_insert_tip()
RETURNS TRIGGER AS $$
BEGIN
	IF (SELECT openstatus 
		FROM Business
		WHERE NEW.bid = Business.bid) = false
	THEN RAISE EXCEPTION 'This business is closed: cannot insert tip';
	END IF;
	RETURN NEW;
END; $$ LANGUAGE 'plpgsql';

CREATE TRIGGER add_tip_open_status
BEFORE INSERT ON Tip
FOR EACH ROW 
EXECUTE PROCEDURE bef_insert_tip();


-- TEST STATEMENTS --

-- insert into Tip table
INSERT INTO TIP (uid, bid, date, text, likes) VALUES
('bvu13GyOUwhEjPum2xjiqQ','qfL7ZFkxqDZOPIaVVfT3Aw','2017-03-21','New test tip', 0);

-- insert or update Checkin on business with open (true) status
INSERT INTO Checkin (bid, day, num_morning, num_afternoon, num_evening, num_night) VALUES ('qfL7ZFkxqDZOPIaVVfT3Aw', 'Tuesday', 0,1,0,0)
ON CONFLICT (bid,day) DO UPDATE SET num_afternoon = Checkin.num_afternoon + 1

-- insert into Tip table where Business open status is false
INSERT INTO TIP (uid, bid, date, text, likes) VALUES
('bvu13GyOUwhEjPum2xjiqQ','cE27W9VPgO88Qxe4ol6y_g','2017-03-21','SHOULD NOT INSERTED', 0);
