INSERT OR IGNORE INTO users(id,username,display_name) VALUES
(1,'Helge','Helge'),
(2,'Adrian','Adrian');

INSERT OR IGNORE INTO messages(id,author_id,text,created_at) VALUES
(1,1,'Hello, BDSA students!','2024-09-01T08:00:00Z'),
(2,2,'Hej, velkommen til kurset.','2024-09-01T08:30:00Z');