INSERT INTO suivi_abonnement_omnis_db.categories (nom)
VALUES 
('Software Development'),
('HR Services'),
('Marketing Campaigns');



INSERT INTO suivi_abonnement_omnis_db.departements (departement_id, nom)
VALUES 
(1, 'DRH'),
(2, 'DSI'),
(3, 'Marketing');



INSERT INTO suivi_abonnement_omnis_db.users (username, email, password, role)
VALUES
('john_doe', 'john.doe@example.com', 'password123', 'admin'),
('jane_doe', 'jane.doe@example.com', 'password123', 'user'),
('cedric_nomena', 'cedricnomena60@gmail.com', 'password123', 'user');


INSERT INTO suivi_abonnement_omnis_db.departement_user (user_id, iddepartement)
VALUES
(1, 1),  -- John Doe associé au département DRH
(2, 2),  -- Jane Doe associé au département DSI
(3, 3);  -- Cedric Nomena associé au département Marketing


INSERT INTO suivi_abonnement_omnis_db.fournisseurs (nom, email, telephone)
VALUES
('Fournisseur A', 'fournisseurA@example.com', '0123456789'),
('Fournisseur B', 'fournisseurB@example.com', '0987654321');


INSERT INTO suivi_abonnement_omnis_db.abonnements (type, prix, idfournisseur, idcategorie, expiration_date, description, nom, departement_id)
VALUES
('Software License', 1000, 1, 1, '2025-12-31 23:59:59', 'Abonnement logiciel de développement', 'Dev Soft', 1),
('HR Service', 500, 2, 2, '2025-11-30 23:59:59', 'Service RH pour l\'entreprise', 'HR Service A', 2),
('Marketing Subscription', 200, 1, 3, '2025-10-15 23:59:59', 'Abonnement pour les campagnes marketing', 'Campaign Marketing', 3);



INSERT INTO suivi_abonnement_omnis_db.notifications (message, type, status, idabonnement)
VALUES
('Abonnement logiciel expiré', 'Alert', 'Inactive', 1),
('Service RH renouvelé', 'Reminder', 'Active', 2),
('Nouvelle campagne marketing lancée', 'Info', 'Active', 3);
