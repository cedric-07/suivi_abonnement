INSERT INTO suivi_abonnement_omnis_db.categories (nom) VALUES
('Internet Haut Débit'), 
('Télévision HD'), 
('Téléphonie Mobile'), 
('Énergie Solaire');





INSERT INTO suivi_abonnement_omnis_db.departements (departement_id, nom) VALUES
(1, 'Direction Générale'),
(2, 'Support Technique'),
(3, 'Marketing'),
(4, 'Service Client'),
(5, 'Ressources Humaines');




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


INSERT INTO suivi_abonnement_omnis_db.fournisseurs (nom, email, telephone) VALUES
('Orange', 'contact@orange.com', '+33 1 70 00 00 00'),
('SFR', 'support@sfr.com', '+33 1 70 50 00 00'),
('Bouygues Telecom', 'contact@bouygues.com', '+33 1 72 72 12 34');



INSERT INTO suivi_abonnement_omnis_db.abonnements (type, prix, date_debut, idfournisseur, idcategorie, expiration_date, description, nom, departement_id) VALUES
('Annuel', 39.99, '2025-01-01 10:00:00', 1, 1, '2026-01-01 10:00:00', 'Abonnement fibre optique 1 Gb/s', 'Fibre Optique Plus', 1),
('Annuel', 25.99, '2025-01-05 10:00:00', 2, 2, '2026-01-05 10:00:00', 'Abonnement TV HD avec 100 chaînes', 'TV Premium', 2),
('Annuel', 19.99, '2025-01-10 10:00:00', 3, 3, '2026-01-10 10:00:00', 'Abonnement mobile 5 Go data + appels illimités', 'Mobile Plus', 3),
('Mensuel', 49.99, '2024-12-01 10:00:00', 1, 1, '2025-01-01 10:00:00', 'Abonnement internet 50 Mb/s', 'Internet Plus', 1),
('Annuel', 29.99, '2024-11-15 10:00:00', 2, 2, '2025-01-01 10:00:00', 'Abonnement TV 4K avec 150 chaînes', 'TV Ultra', 2);


--Abonnement expirer Admin---
-- Insérer plusieurs abonnements dont l'expiration est dans le premier mois
INSERT INTO suivi_abonnement_omnis_db.abonnements (type, prix, date_debut, idfournisseur, idcategorie, expiration_date, description, nom, departement_id) 
VALUES
('Mensuel', 59.99, CURRENT_TIMESTAMP, 1, 1, '2025-03-01 10:00:00' , 'Abonnement Internet avec débit élevé', 'Internet Pro', 1),
('Mensuel', 39.99, CURRENT_TIMESTAMP, 2, 2, '2025-03-01 10:00:00', 'Abonnement TV HD avec 100 chaînes', 'TV Plus', 2),
('Mensuel', 29.99, CURRENT_TIMESTAMP, 3, 3, '2025-03-01 10:00:00', 'Abonnement Mobile 5 Go de données', 'Mobile Standard', 3),
('Mensuel', 79.99, CURRENT_TIMESTAMP, 1, 1, '2025-03-01 10:00:00', 'Abonnement fibre optique 1 Gb/s', 'Fibre Optique', 1),
('Mensuel', 39.99, CURRENT_TIMESTAMP, 3, 3, '2025-03-01 10:00:00', 'Abonnement mobile 10 Go de données', 'Mobile Plus', 5);

--Abonnement expirer User---

-- Insérer plusieurs abonnements dont l'expiration est dans un mois
INSERT INTO suivi_abonnement_omnis_db.abonnements (type, prix, date_debut, idfournisseur, idcategorie, expiration_date, description, nom, departement_id) 
VALUES
('Mensuel', 39.99, CURRENT_TIMESTAMP, 2, 2, '2025-03-01 10:00:00', 'Abonnement TV HD avec 100 chaînes', 'TV Standard', 1),
('Mensuel', 29.99, CURRENT_TIMESTAMP, 3, 3, '2025-03-01 10:00:00', 'Abonnement Mobile 5 Go de données', 'Mobile Basic', 3),
('Mensuel', 79.99, CURRENT_TIMESTAMP, 1, 1, '2025-03-01 10:00:00', 'Abonnement fibre optique 1 Gb/s', 'Fibre Plus', 1),
('Mensuel', 49.99, CURRENT_TIMESTAMP, 2, 2, '2025-03-01 10:00:00', 'Abonnement TV 4K avec 150 chaînes', 'TV Premium', 2);


--Abonnement expirer 7 jours--
INSERT INTO suivi_abonnement_omnis_db.abonnements (type, prix, date_debut, idfournisseur, idcategorie, expiration_date, description, nom, departement_id) 
VALUES
('Mensuel', 49.99, CURRENT_TIMESTAMP, 1, 1, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 7 DAY), 'Abonnement Internet avec débit élevé', 'Internet Pro', 1),
('Mensuel', 69.99, CURRENT_TIMESTAMP, 2, 2, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 7 DAY), 'Abonnement TV HD avec 100 chaînes', 'TV Plus', 2),
('Mensuel', 79.99, CURRENT_TIMESTAMP, 3, 3, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 7 DAY), 'Abonnement Mobile 5 Go de données', 'Mobile Standard', 3),
('Mensuel', 59.99, CURRENT_TIMESTAMP, 1, 1, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 7 DAY), 'Abonnement fibre optique 1 Gb/s', 'Fibre Optique', 1),
('Mensuel', 79.99, CURRENT_TIMESTAMP, 3, 3, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 7 DAY), 'Abonnement mobile 10 Go de données', 'Mobile Plus', 5);



INSERT INTO suivi_abonnement_omnis_db.notifications (message, type, status, idabonnement, iduser, created_at) VALUES
('Votre abonnement Fibre Optique Plus est activé et prêt à l\'emploi.', 'Confirmation', 'Lu', 1, 2, '2025-01-01 10:30:00'),
('Votre abonnement TV Premium a été activé avec succès.', 'Confirmation', 'Non Lu', 2, 1, '2025-01-05 11:00:00'),
('Abonnement Mobile Plus activé. Profitez de vos 5 Go de données.', 'Confirmation', 'Non Lu', 3, 3, '2025-01-10 12:00:00');

