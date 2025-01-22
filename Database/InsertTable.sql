INSERT INTO suivi_abonnement_omnis_db.categories (categorie_id, nom) VALUES
(1, 'Internet'),
(2, 'Ecran plat'),
(3, 'Projecteur'),
(4, 'Logiciel');

INSERT INTO suivi_abonnement_omnis_db.fournisseurs (fournisseur_id, nom, email, telephone) VALUES
(1, 'Orange', 'contact@orange.com', '0123456789'),
(2, 'Telma', 'telmaMadagascar@mg.com', '0987654321'),
(3, 'Raitra project', 'raitrapr@mada.com', '0147852369');

INSERT INTO suivi_abonnement_omnis_db.abonnements (abonnement_id, nom, description, `type`, prix, date_debut,  idfournisseur, idcategorie, expiration_date) VALUES
(1, 'Abonnement Internet', 'Abonnement Internet haut d�bit', "Annuel", 30.00, '2024-01-01 12:00:00', 1,  1, '2025-01-01 12:00:00'),
(2, 'Abonnement adobe', 'Abonnement adobe avec des outils', "Annuel", 50.00, '2024-02-01 12:00:00', 2,  2, '2025-02-01 12:00:00');

INSERT INTO suivi_abonnement_omnis_db.notifications (message, `type`, `status`, idabonnement) VALUES
('Votre abonnement Internet a �t� activ�.', 'info', 'active', 1),
('Votre abonnement T�l�vision sera renouvel�.', 'reminder', 'active', 2);

