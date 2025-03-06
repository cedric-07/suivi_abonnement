CREATE SCHEMA suivi_abonnement_omnis_db;

CREATE  TABLE suivi_abonnement_omnis_db.categories ( 
	categorie_id         INT    NOT NULL AUTO_INCREMENT  PRIMARY KEY,
	nom                  VARCHAR(250)    NOT NULL   
 ) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE  TABLE suivi_abonnement_omnis_db.directions ( 
	direction_id         INT    NOT NULL   PRIMARY KEY,
	nom                  VARCHAR(250)       
 ) engine=InnoDB;

CREATE  TABLE suivi_abonnement_omnis_db.fournisseurs ( 
	fournisseur_id       INT    NOT NULL AUTO_INCREMENT  PRIMARY KEY,
	nom                  VARCHAR(250)    NOT NULL   ,
	email                VARCHAR(50)    NOT NULL   ,
	telephone            VARCHAR(20)    NOT NULL   
 ) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE  TABLE suivi_abonnement_omnis_db.users ( 
	id                   INT    NOT NULL AUTO_INCREMENT  PRIMARY KEY,
	username             VARCHAR(250)    NOT NULL   ,
	email                VARCHAR(50)    NOT NULL   ,
	password             VARCHAR(250)    NOT NULL   ,
	password_reset_token VARCHAR(250)       ,
	role                 VARCHAR(20)       ,
	isconnected          BOOLEAN  DEFAULT (false)     
 ) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE  TABLE suivi_abonnement_omnis_db.conversations ( 
	conversation_id      INT    NOT NULL AUTO_INCREMENT  PRIMARY KEY,
	user1_id             INT    NOT NULL   ,
	user2_id             INT    NOT NULL   ,
	`LastMessageat`      TIMESTAMP  DEFAULT (current_timestamp())  NOT NULL   
 ) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE INDEX user1_id ON suivi_abonnement_omnis_db.conversations ( user1_id );

CREATE INDEX user2_id ON suivi_abonnement_omnis_db.conversations ( user2_id );

CREATE  TABLE suivi_abonnement_omnis_db.departements ( 
	departement_id       INT    NOT NULL   PRIMARY KEY,
	nom                  VARCHAR(250)       ,
	iddirection          INT       
 ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE  TABLE suivi_abonnement_omnis_db.messages ( 
	message_id           INT    NOT NULL AUTO_INCREMENT  PRIMARY KEY,
	messagetext          TEXT    NOT NULL   ,
	sentat               TIMESTAMP  DEFAULT (current_timestamp())  NOT NULL   ,
	isread               BOOLEAN  DEFAULT (false)     ,
	senderid             INT    NOT NULL   ,
	receiverid           INT    NOT NULL   ,
	idconversation       INT    NOT NULL   ,
	file_path            VARCHAR(500)  DEFAULT ('null')     ,
	message_type         INT       
 ) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE INDEX senderid ON suivi_abonnement_omnis_db.messages ( senderid );

CREATE INDEX receiverid ON suivi_abonnement_omnis_db.messages ( receiverid );

CREATE INDEX idconversation ON suivi_abonnement_omnis_db.messages ( idconversation );

CREATE  TABLE suivi_abonnement_omnis_db.abonnements ( 
	abonnement_id        INT    NOT NULL AUTO_INCREMENT  PRIMARY KEY,
	`type`               VARCHAR(50)    NOT NULL   ,
	prix                 DECIMAL(10,0)    NOT NULL   ,
	date_debut           TIMESTAMP  DEFAULT (current_timestamp()) ON UPDATE current_timestamp NOT NULL   ,
	idfournisseur        INT    NOT NULL   ,
	idcategorie          INT    NOT NULL   ,
	expiration_date      TIMESTAMP  DEFAULT (current_timestamp())  NOT NULL   ,
	description          VARCHAR(500)       ,
	nom                  VARCHAR(250)    NOT NULL   ,
	departement_id       INT    NOT NULL   
 ) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE INDEX unq_abonnements_idcategorie ON suivi_abonnement_omnis_db.abonnements ( idcategorie );

CREATE INDEX fk_abonnements_fournisseurs ON suivi_abonnement_omnis_db.abonnements ( idfournisseur );

CREATE INDEX fk_abonnements_departements ON suivi_abonnement_omnis_db.abonnements ( departement_id );

CREATE  TABLE suivi_abonnement_omnis_db.departement_user ( 
	id                   INT    NOT NULL AUTO_INCREMENT  PRIMARY KEY,
	user_id              INT    NOT NULL   ,
	iddepartement        INT       
 ) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE INDEX fk_users_departements ON suivi_abonnement_omnis_db.departement_user ( user_id );

CREATE INDEX fk_departement_user ON suivi_abonnement_omnis_db.departement_user ( iddepartement );

CREATE  TABLE suivi_abonnement_omnis_db.notifications ( 
	notification_id      BIGINT    NOT NULL AUTO_INCREMENT  PRIMARY KEY,
	message              VARCHAR(250)    NOT NULL   ,
	`type`               VARCHAR(20)    NOT NULL   ,
	`status`             VARCHAR(20)    NOT NULL   ,
	idabonnement         INT    NOT NULL   ,
	iduser               INT       ,
	created_at           TIMESTAMP  DEFAULT (current_timestamp()) ON UPDATE current_timestamp NOT NULL   
 ) ENGINE=InnoDB AUTO_INCREMENT=54 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE INDEX fk_notifications_abonnements ON suivi_abonnement_omnis_db.notifications ( idabonnement );

CREATE INDEX fk_notifications_users ON suivi_abonnement_omnis_db.notifications ( iduser );

ALTER TABLE suivi_abonnement_omnis_db.abonnements ADD CONSTRAINT fk_abonnements_categories FOREIGN KEY ( idcategorie ) REFERENCES suivi_abonnement_omnis_db.categories( categorie_id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.abonnements ADD CONSTRAINT fk_abonnements_departements FOREIGN KEY ( departement_id ) REFERENCES suivi_abonnement_omnis_db.departements( departement_id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.abonnements ADD CONSTRAINT fk_abonnements_fournisseurs FOREIGN KEY ( idfournisseur ) REFERENCES suivi_abonnement_omnis_db.fournisseurs( fournisseur_id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.conversations ADD CONSTRAINT conversations_ibfk_1 FOREIGN KEY ( user1_id ) REFERENCES suivi_abonnement_omnis_db.users( id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.conversations ADD CONSTRAINT conversations_ibfk_2 FOREIGN KEY ( user2_id ) REFERENCES suivi_abonnement_omnis_db.users( id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.departement_user ADD CONSTRAINT fk_departement_user FOREIGN KEY ( iddepartement ) REFERENCES suivi_abonnement_omnis_db.departements( departement_id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.departement_user ADD CONSTRAINT fk_users_departements FOREIGN KEY ( user_id ) REFERENCES suivi_abonnement_omnis_db.users( id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.departements ADD CONSTRAINT fk_departements_directions FOREIGN KEY ( iddirection ) REFERENCES suivi_abonnement_omnis_db.directions( direction_id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.messages ADD CONSTRAINT messages_ibfk_1 FOREIGN KEY ( senderid ) REFERENCES suivi_abonnement_omnis_db.users( id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.messages ADD CONSTRAINT messages_ibfk_2 FOREIGN KEY ( receiverid ) REFERENCES suivi_abonnement_omnis_db.users( id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.messages ADD CONSTRAINT messages_ibfk_3 FOREIGN KEY ( idconversation ) REFERENCES suivi_abonnement_omnis_db.conversations( conversation_id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.notifications ADD CONSTRAINT fk_notifications_abonnements FOREIGN KEY ( idabonnement ) REFERENCES suivi_abonnement_omnis_db.abonnements( abonnement_id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE suivi_abonnement_omnis_db.notifications ADD CONSTRAINT fk_notifications_users FOREIGN KEY ( iduser ) REFERENCES suivi_abonnement_omnis_db.users( id ) ON DELETE NO ACTION ON UPDATE NO ACTION;

CREATE VIEW suivi_abonnement_omnis_db.v_abonnements_par_client AS select `u`.`id` AS `idclient`,`u`.`username` AS `nomclient`,`u`.`email` AS `emailclient`,`u`.`role` AS `roleclient`,`a`.`abonnement_id` AS `abonnement_id`,`a`.`type` AS `type`,`a`.`prix` AS `prix`,`a`.`date_debut` AS `date_debut`,`a`.`expiration_date` AS `expiration_date`,`a`.`description` AS `description`,`a`.`nom` AS `nomabonnement`,`d`.`nom` AS `nomdepartement`,`f`.`nom` AS `nomfournisseur` from ((((`suivi_abonnement_omnis_db`.`users` `u` join `suivi_abonnement_omnis_db`.`departement_user` `du` on(`u`.`id` = `du`.`user_id`)) join `suivi_abonnement_omnis_db`.`departements` `d` on(`du`.`iddepartement` = `d`.`departement_id`)) join `suivi_abonnement_omnis_db`.`abonnements` `a` on(`a`.`departement_id` = `du`.`iddepartement`)) join `suivi_abonnement_omnis_db`.`fournisseurs` `f` on(`a`.`idfournisseur` = `f`.`fournisseur_id`)) order by `u`.`id`,`a`.`abonnement_id`;

CREATE VIEW suivi_abonnement_omnis_db.v_abonnements_user AS select `u`.`id` AS `user_id`,`u`.`username` AS `username`,`u`.`email` AS `email`,`dep`.`nom` AS `departement`,`a`.`abonnement_id` AS `abonnement_id`,`a`.`nom` AS `abonnement_nom`,`a`.`type` AS `abonnement_type`,`a`.`prix` AS `prix`,`a`.`date_debut` AS `date_debut`,`a`.`expiration_date` AS `expiration_date`,`a`.`description` AS `abonnement_description`,`f`.`nom` AS `fournisseur_nom`,`c`.`nom` AS `categorie_nom` from (((((`suivi_abonnement_omnis_db`.`users` `u` join `suivi_abonnement_omnis_db`.`departement_user` `du` on(`u`.`id` = `du`.`user_id`)) join `suivi_abonnement_omnis_db`.`abonnements` `a` on(`du`.`iddepartement` = `a`.`departement_id`)) join `suivi_abonnement_omnis_db`.`fournisseurs` `f` on(`a`.`idfournisseur` = `f`.`fournisseur_id`)) join `suivi_abonnement_omnis_db`.`categories` `c` on(`a`.`idcategorie` = `c`.`categorie_id`)) join `suivi_abonnement_omnis_db`.`departements` `dep` on(`du`.`iddepartement` = `dep`.`departement_id`)) group by `u`.`id`,`a`.`abonnement_id` order by `u`.`id`,`a`.`abonnement_id`;

CREATE VIEW suivi_abonnement_omnis_db.v_status_abonnements AS select `a`.`abonnement_id` AS `abonnement_id`,`a`.`type` AS `type`,`a`.`prix` AS `prix`,`a`.`date_debut` AS `date_debut`,`a`.`idfournisseur` AS `idfournisseur`,`a`.`idcategorie` AS `idcategorie`,`a`.`expiration_date` AS `expiration_date`,`a`.`description` AS `description`,`a`.`nom` AS `nom`,`a`.`departement_id` AS `departement_id`,`c`.`nom` AS `nom_categorie`,`f`.`nom` AS `nom_fournisseur`,`d`.`nom` AS `nom_departement`,case when `a`.`expiration_date` > current_timestamp() and `a`.`date_debut` < current_timestamp() then 'Actif' when `a`.`expiration_date` < current_timestamp() then 'Expire' else 'En attente' end AS `statut` from (((`suivi_abonnement_omnis_db`.`abonnements` `a` join `suivi_abonnement_omnis_db`.`departements` `d` on(`a`.`departement_id` = `d`.`departement_id`)) join `suivi_abonnement_omnis_db`.`categories` `c` on(`a`.`idcategorie` = `c`.`categorie_id`)) join `suivi_abonnement_omnis_db`.`fournisseurs` `f` on(`a`.`idfournisseur` = `f`.`fournisseur_id`));
