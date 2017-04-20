
-- Calculates and updates the 'numcheckins' and 'review_count' information for each business. 
-- Queries the tips and checkin tables to calculate the total number of checkins and number of tips for each business.

-- 'numcheckins' value for a business is updated to the sum of all check-ins for that business. 
-- UPDATE Business
-- SET numcheckins = 
-- 	(
-- 		SELECT SUM(c.sum)
-- 		FROM (
-- 				SELECT (num_morning + num_afternoon + num_evening + num_night) as sum
-- 				FROM Checkin
-- 				WHERE Checkin.bid = Business.bid
-- 			 ) AS c
-- 	);

UPDATE Business
SET numcheckins = TotalCheckIns.total
FROM 
(
	SELECT bid, SUM(num_morning)+SUM(num_afternoon)+SUM(num_evening)+SUM(num_night) as total
	FROM CheckIn
	GROUP BY bid
) as TotalCheckIns
WHERE Business.bid = TotalCheckIns.bid;


-- 'review_count' is updated to the number of tips provided for that business.
-- UPDATE Business
-- SET review_count = 
-- 	(
-- 		SELECT COUNT(*)
-- 		FROM Tip
-- 		WHERE Tip.bid = Business.bid
-- 	);

UPDATE Business
SET review_count  = TotalReviews.totalReviews
FROM 
(
	SELECT bid, count(*) as total
	FROM Tip
	GROUP BY bid
) as TotalReviews
WHERE Business.bid = TotalReviews.bid;