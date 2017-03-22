
-- Calculates and updates the 'numcheckins' and 'review_count' information for each business. 
-- Queries the tips and checkin tables to calculate the total number of checkins and number of tips for each business.

-- 'numcheckins' value for a business is updated to the sum of all check-ins for that business. 
UPDATE Business
SET numcheckins = 
	(
		SELECT SUM(c.sum)
		FROM (
				SELECT (num_morning + num_afternoon + num_evening + num_night) as sum
				FROM Checkin
				WHERE Checkin.bid = Business.bid
			 ) AS c
	);

-- 'review_count' is updated to the number of tips provided for that business.
UPDATE Business
SET review_count = 
	(
		SELECT COUNT(*)
		FROM Tip
		WHERE Tip.bid = Business.bid
	);