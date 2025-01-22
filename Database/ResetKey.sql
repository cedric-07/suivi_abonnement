DELETE FROM suivi_abonnement_omnis_db.notifications;
DELETE FROM suivi_abonnement_omnis_db.abonnements;
DELETE FROM suivi_abonnement_omnis_db.departement_user;
DELETE FROM suivi_abonnement_omnis_db.users;
DELETE FROM suivi_abonnement_omnis_db.fournisseurs;
DELETE FROM suivi_abonnement_omnis_db.categories;
DELETE FROM suivi_abonnement_omnis_db.departements;

ALTER TABLE suivi_abonnement_omnis_db.notifications AUTO_INCREMENT = 1;
ALTER TABLE suivi_abonnement_omnis_db.abonnements AUTO_INCREMENT = 1;
ALTER TABLE suivi_abonnement_omnis_db.departement_user AUTO_INCREMENT = 1;
ALTER TABLE suivi_abonnement_omnis_db.users AUTO_INCREMENT = 1;
ALTER TABLE suivi_abonnement_omnis_db.fournisseurs AUTO_INCREMENT = 1;
ALTER TABLE suivi_abonnement_omnis_db.categories AUTO_INCREMENT = 1;
ALTER TABLE suivi_abonnement_omnis_db.departements AUTO_INCREMENT = 1;
