
-- Remove other 'processes'
SELECT pid, pg_terminate_backend(pid) 
FROM pg_stat_activity 
WHERE datname = current_database() AND pid <> pg_backend_pid();

-- insert into Tip table
INSERT INTO TIP (uid, bid, date, text, likes) VALUES
('bvu13GyOUwhEjPum2xjiqQ','qfL7ZFkxqDZOPIaVVfT3Aw','2017-03-21','FOOl', 0);

-- insert or update Checkin
INSERT INTO Checkin (bid, day, num_morning, num_afternoon, num_evening, num_night)VALUES
('qfL7ZFkxqDZOPIaVVfT3Aw', 'Tuesday', 0,1,0,0)
ON CONFLICT (bid,day) DO UPDATE SET num_afternoon = Checkin.num_afternoon + 1