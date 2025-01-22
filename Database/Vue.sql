CREATE VIEW v_abonnements_user AS
SELECT 
    u.id AS user_id,
    u.username,
    u.email,
    u.role,
    dep.nom AS departement,
    a.abonnement_id,
    a.nom AS abonnement_nom,
    a.type AS abonnement_type,
    a.prix,
    a.date_debut,
    a.expiration_date,
    a.description AS abonnement_description,
    f.nom AS fournisseur_nom,
    c.nom AS categorie_nom
FROM 
    suivi_abonnement_omnis_db.users u
JOIN 
    suivi_abonnement_omnis_db.departement_user du ON u.id = du.user_id
JOIN 
    suivi_abonnement_omnis_db.abonnements a ON du.iddepartement = a.departement_id
JOIN 
    suivi_abonnement_omnis_db.fournisseurs f ON a.idfournisseur = f.fournisseur_id
JOIN 
    suivi_abonnement_omnis_db.categories c ON a.idcategorie = c.categorie_id
JOIN 
    suivi_abonnement_omnis_db.departements dep ON du.iddepartement = dep.departement_id
GROUP BY 
    u.id, a.abonnement_id
ORDER BY u.id , a.abonnement_id;



CREATE VIEW v_abonnements AS
SELECT 
    a.abonnement_id,
    a.nom AS abonnement_nom,
    a.type AS abonnement_type,
    a.prix,
    a.date_debut,
    a.expiration_date,
    a.description AS abonnement_description,
    u.username AS utilisateur_nom,
    u.email AS utilisateur_email,
    f.nom AS fournisseur_nom,
    c.nom AS categorie_nom,
    dep.nom AS departement_nom
FROM 
    suivi_abonnement_omnis_db.abonnements a
JOIN 
    suivi_abonnement_omnis_db.fournisseurs f ON a.idfournisseur = f.fournisseur_id
JOIN 
    suivi_abonnement_omnis_db.categories c ON a.idcategorie = c.categorie_id
JOIN 
    suivi_abonnement_omnis_db.departement_user du ON a.departement_id = du.iddepartement
JOIN 
    suivi_abonnement_omnis_db.users u ON du.user_id = u.id
JOIN 
    suivi_abonnement_omnis_db.departements dep ON du.iddepartement = dep.departement_id;









CREATE VIEW v_abonnements_par_client AS
SELECT 
    u.id AS idclient,
    u.username AS nomclient,
    u.email AS emailclient,
    u.role AS roleclient,
    a.abonnement_id,
    a.type AS type,
    a.prix AS prix,
    a.date_debut,
    a.expiration_date,
    a.description,
    a.nom AS nomabonnement,
    d.nom AS nomdepartement,
    f.nom AS nomfournisseur
FROM 
    suivi_abonnement_omnis_db.users u
JOIN 
    suivi_abonnement_omnis_db.departement_user du ON u.id = du.user_id
JOIN 
    suivi_abonnement_omnis_db.departements d ON du.iddepartement = d.departement_id
JOIN 
    suivi_abonnement_omnis_db.abonnements a ON a.departement_id = du.iddepartement  -- Correction ici
JOIN
    suivi_abonnement_omnis_db.fournisseurs f ON a.idfournisseur = f.fournisseur_id
ORDER BY u.id, a.abonnement_id;

