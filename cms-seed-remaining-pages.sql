-- CMS Content Blocks Seed: Remaining Pages
-- (Auth, Questionnaire, Create Plan, Subscription, Onboarding, Legal, Global Navigation)
-- Version ID: 17a4a74e-4782-4ca0-9493-aebbd22dcc95
-- Block Types: Text=0, RichText=1, Image=2, Link=3, Json=4, Number=5, Boolean=6

-- ============================================================
-- AUTH - LOGIN (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.title',0,'Welcome back','en',1,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.subtitle',0,'Sign in to continue to your dashboard','en',2,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.email_label',0,'Email Address','en',3,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.password_label',0,'Password','en',4,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.button',0,'Sign In','en',5,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.signing_in',0,'Signing in...','en',6,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.forgot_password',0,'Forgot password?','en',7,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.divider',0,'or continue with','en',8,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.google_button',0,'Continue with Google','en',9,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.no_account',0,'Don''t have an account?','en',10,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.create_account',0,'Create one now','en',11,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.back_to_home',0,'Back to home','en',12,'auth.login',NULL,NOW(),NOW(),false);

-- AUTH - LOGIN (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.title',0,'Bon retour','fr',1,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.subtitle',0,E'Connectez-vous pour acc\u00e9der \u00e0 votre tableau de bord','fr',2,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.email_label',0,'Adresse e-mail','fr',3,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.password_label',0,'Mot de passe','fr',4,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.button',0,'Se connecter','fr',5,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.signing_in',0,'Connexion en cours...','fr',6,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.forgot_password',0,E'Mot de passe oubli\u00e9?','fr',7,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.divider',0,'ou continuer avec','fr',8,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.google_button',0,'Continuer avec Google','fr',9,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.no_account',0,E'Vous n''avez pas de compte?','fr',10,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.create_account',0,E'Cr\u00e9er un compte','fr',11,'auth.login',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.login.back_to_home',0,E'Retour \u00e0 l''accueil','fr',12,'auth.login',NULL,NOW(),NOW(),false);

-- ============================================================
-- AUTH - SIGNUP (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.title',0,'Create your account','en',1,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.subtitle',0,'Get started with your free 14-day trial','en',2,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.firstname_label',0,'First Name','en',3,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.lastname_label',0,'Last Name','en',4,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.email_label',0,'Email Address','en',5,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.password_label',0,'Password','en',6,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.confirm_label',0,'Confirm Password','en',7,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.terms_prefix',0,'I agree to the','en',8,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.terms_link',0,'Terms of Service','en',9,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.privacy_link',0,'Privacy Policy','en',10,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.button',0,'Create Account','en',11,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.creating',0,'Creating account...','en',12,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.has_account',0,'Already have an account?','en',13,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.signin_link',0,'Sign in instead','en',14,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.divider',0,'or continue with','en',15,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.google_button',0,'Continue with Google','en',16,'auth.signup',NULL,NOW(),NOW(),false);

-- AUTH - SIGNUP (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.title',0,E'Cr\u00e9er votre compte','fr',1,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.subtitle',0,'Commencez avec votre essai gratuit de 14 jours','fr',2,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.firstname_label',0,E'Pr\u00e9nom','fr',3,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.lastname_label',0,'Nom','fr',4,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.email_label',0,'Adresse e-mail','fr',5,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.password_label',0,'Mot de passe','fr',6,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.confirm_label',0,'Confirmer le mot de passe','fr',7,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.terms_prefix',0,E'J''accepte les','fr',8,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.terms_link',0,E'Conditions d''utilisation','fr',9,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.privacy_link',0,E'Politique de confidentialit\u00e9','fr',10,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.button',0,E'Cr\u00e9er un compte','fr',11,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.creating',0,E'Cr\u00e9ation du compte...','fr',12,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.has_account',0,E'Vous avez d\u00e9j\u00e0 un compte?','fr',13,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.signin_link',0,'Se connecter','fr',14,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.divider',0,'ou continuer avec','fr',15,'auth.signup',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.signup.google_button',0,'Continuer avec Google','fr',16,'auth.signup',NULL,NOW(),NOW(),false);

-- ============================================================
-- AUTH - FORGOT PASSWORD (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.title',0,'Forgot password?','en',1,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.subtitle',0,'No worries! Enter your email and we''ll send you a reset link.','en',2,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.email_label',0,'Email Address','en',3,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.button',0,'Send Reset Link','en',4,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.sending',0,'Sending...','en',5,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.back_to_login',0,'Back to Login','en',6,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.success_title',0,'Check your email','en',7,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.success_message',0,'We''ve sent a password reset link to your email address.','en',8,'auth.forgot_password',NULL,NOW(),NOW(),false);

-- AUTH - FORGOT PASSWORD (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.title',0,E'Mot de passe oubli\u00e9 ?','fr',1,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.subtitle',0,E'Pas de souci ! Entrez votre courriel et nous vous enverrons un lien de r\u00e9initialisation.','fr',2,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.email_label',0,'Adresse e-mail','fr',3,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.button',0,'Envoyer le lien','fr',4,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.sending',0,'Envoi en cours...','fr',5,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.back_to_login',0,E'Retour \u00e0 la connexion','fr',6,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.success_title',0,E'V\u00e9rifiez votre courriel','fr',7,'auth.forgot_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.forgot_password.success_message',0,E'Nous avons envoy\u00e9 un lien de r\u00e9initialisation \u00e0 votre adresse courriel.','fr',8,'auth.forgot_password',NULL,NOW(),NOW(),false);

-- ============================================================
-- AUTH - RESET PASSWORD (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.title',0,'Set new password','en',1,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.subtitle',0,'Enter your new password below.','en',2,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.new_password_label',0,'New Password','en',3,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.confirm_password_label',0,'Confirm New Password','en',4,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.button',0,'Reset Password','en',5,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.resetting',0,'Resetting password...','en',6,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.back_to_login',0,'Back to Login','en',7,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.success_title',0,'Password reset successful!','en',8,'auth.reset_password',NULL,NOW(),NOW(),false);

-- AUTH - RESET PASSWORD (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.title',0,'Nouveau mot de passe','fr',1,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.subtitle',0,'Entrez votre nouveau mot de passe ci-dessous.','fr',2,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.new_password_label',0,'Nouveau mot de passe','fr',3,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.confirm_password_label',0,'Confirmer le nouveau mot de passe','fr',4,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.button',0,E'R\u00e9initialiser le mot de passe','fr',5,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.resetting',0,E'R\u00e9initialisation en cours...','fr',6,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.back_to_login',0,E'Retour \u00e0 la connexion','fr',7,'auth.reset_password',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.reset_password.success_title',0,E'Mot de passe r\u00e9initialis\u00e9 !','fr',8,'auth.reset_password',NULL,NOW(),NOW(),false);

-- ============================================================
-- AUTH - VERIFY EMAIL (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.loading_title',0,'Verifying your email...','en',1,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.success_title',0,'Email verified!','en',2,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.success_message',0,'Your email has been verified. You can now log in.','en',3,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.error_title',0,'Verification failed','en',4,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.resend_button',0,'Resend Verification Email','en',5,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.resend_sending',0,'Sending...','en',6,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.login_link',0,'Log in','en',7,'auth.verify_email',NULL,NOW(),NOW(),false);

-- AUTH - VERIFY EMAIL (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.loading_title',0,E'V\u00e9rification de votre courriel...','fr',1,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.success_title',0,E'Courriel v\u00e9rifi\u00e9 !','fr',2,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.success_message',0,E'Votre courriel a \u00e9t\u00e9 v\u00e9rifi\u00e9. Vous pouvez maintenant vous connecter.','fr',3,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.error_title',0,E'\u00c9chec de la v\u00e9rification','fr',4,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.resend_button',0,E'Renvoyer le courriel de v\u00e9rification','fr',5,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.resend_sending',0,'Envoi en cours...','fr',6,'auth.verify_email',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','auth.verify_email.login_link',0,'Se connecter','fr',7,'auth.verify_email',NULL,NOW(),NOW(),false);

-- ============================================================
-- QUESTIONNAIRE - STEPS (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_1_title',0,'Identity & Vision','en',1,'questionnaire.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_2_title',0,'The Offering','en',2,'questionnaire.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_3_title',0,'Market Analysis','en',3,'questionnaire.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_4_title',0,'Operations & People','en',4,'questionnaire.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_5_title',0,'Financials & Risks','en',5,'questionnaire.steps',NULL,NOW(),NOW(),false);

-- QUESTIONNAIRE - STEPS (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_1_title',0,E'Identit\u00e9 et Vision','fr',1,'questionnaire.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_2_title',0,E'L''Offre','fr',2,'questionnaire.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_3_title',0,E'Analyse du March\u00e9','fr',3,'questionnaire.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_4_title',0,E'Op\u00e9rations et \u00c9quipe','fr',4,'questionnaire.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.step_5_title',0,E'Finances et Risques','fr',5,'questionnaire.steps',NULL,NOW(),NOW(),false);

-- ============================================================
-- QUESTIONNAIRE - LABELS (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.sections',0,'Sections','en',1,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.progress',0,'Progress','en',2,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.ai_suggestion',0,'AI Suggestion','en',3,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.auto_saved',0,'Auto-saved','en',4,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.previous_section',0,'Previous Section','en',5,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.next_section',0,'Next Section','en',6,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.generate_plan',0,'Generate Business Plan','en',7,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.save_now',0,'Save Now','en',8,'questionnaire.labels',NULL,NOW(),NOW(),false);

-- QUESTIONNAIRE - LABELS (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.sections',0,'Sections','fr',1,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.progress',0,'Progression','fr',2,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.ai_suggestion',0,'Suggestion IA','fr',3,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.auto_saved',0,E'Enregistr\u00e9 automatiquement','fr',4,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.previous_section',0,E'Section pr\u00e9c\u00e9dente','fr',5,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.next_section',0,'Section suivante','fr',6,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.generate_plan',0,E'G\u00e9n\u00e9rer le plan d''affaires','fr',7,'questionnaire.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.save_now',0,'Enregistrer maintenant','fr',8,'questionnaire.labels',NULL,NOW(),NOW(),false);

-- ============================================================
-- QUESTIONNAIRE - GENERATION TIPS (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.generation.title',0,'Generating Your Business Plan','en',1,'questionnaire.tips',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.generation.subtitle',0,'This may take 2-5 minutes...','en',2,'questionnaire.tips',NULL,NOW(),NOW(),false);

-- QUESTIONNAIRE - GENERATION TIPS (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.generation.title',0,E'G\u00e9n\u00e9ration de votre plan d''affaires','fr',1,'questionnaire.tips',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','questionnaire.generation.subtitle',0,E'Cela peut prendre 2 \u00e0 5 minutes...','fr',2,'questionnaire.tips',NULL,NOW(),NOW(),false);

-- ============================================================
-- CREATE PLAN - LABELS (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.header_title',0,'Create Your Business Plan','en',1,'create_plan.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.plan_title',0,'Plan Title','en',2,'create_plan.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.start_questionnaire',0,'Start Questionnaire','en',3,'create_plan.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.back',0,'Back to Dashboard','en',4,'create_plan.labels',NULL,NOW(),NOW(),false);

-- CREATE PLAN - LABELS (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.header_title',0,E'Cr\u00e9ez votre plan d''affaires','fr',1,'create_plan.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.plan_title',0,'Titre du plan','fr',2,'create_plan.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.start_questionnaire',0,E'D\u00e9marrer le questionnaire','fr',3,'create_plan.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.back',0,'Retour au tableau de bord','fr',4,'create_plan.labels',NULL,NOW(),NOW(),false);

-- ============================================================
-- CREATE PLAN - TYPES (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.business_plan_title',0,'Business Plan','en',1,'create_plan.types',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.business_plan_desc',0,'Perfect for startups and established businesses seeking funding or strategic direction.','en',2,'create_plan.types',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.obnl_plan_title',0,'OBNL Strategic Plan','en',3,'create_plan.types',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.obnl_plan_desc',0,'Designed for non-profit organizations with mission-driven strategic planning.','en',4,'create_plan.types',NULL,NOW(),NOW(),false);

-- CREATE PLAN - TYPES (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.business_plan_title',0,E'Plan d''affaires','fr',1,'create_plan.types',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.business_plan_desc',0,E'Parfait pour les startups et les entreprises \u00e9tablies cherchant du financement ou une direction strat\u00e9gique.','fr',2,'create_plan.types',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.obnl_plan_title',0,E'Plan strat\u00e9gique OBNL','fr',3,'create_plan.types',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','create_plan.obnl_plan_desc',0,E'Con\u00e7u pour les organismes \u00e0 but non lucratif avec une planification strat\u00e9gique ax\u00e9e sur la mission.','fr',4,'create_plan.types',NULL,NOW(),NOW(),false);

-- ============================================================
-- SUBSCRIPTION - LABELS (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.page_title',0,'Subscription Plans','en',1,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.monthly',0,'Monthly','en',2,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.yearly',0,'Yearly','en',3,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.popular',0,'Most Popular','en',4,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.get_started',0,'Get Started','en',5,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.current_plan',0,'Current Plan','en',6,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.upgrade',0,'Upgrade','en',7,'subscription.labels',NULL,NOW(),NOW(),false);

-- SUBSCRIPTION - LABELS (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.page_title',0,E'Plans d''abonnement','fr',1,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.monthly',0,'Mensuel','fr',2,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.yearly',0,'Annuel','fr',3,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.popular',0,'Le plus populaire','fr',4,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.get_started',0,'Commencer','fr',5,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.current_plan',0,'Plan actuel','fr',6,'subscription.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','subscription.upgrade',0,E'Mettre \u00e0 jour','fr',7,'subscription.labels',NULL,NOW(),NOW(),false);

-- ============================================================
-- ONBOARDING - WELCOME (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.welcome_title',0,'Welcome to Sqordia!','en',1,'onboarding.welcome',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.welcome_subtitle',0,'Let''s set up your experience','en',2,'onboarding.welcome',NULL,NOW(),NOW(),false);

-- ONBOARDING - WELCOME (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.welcome_title',0,'Bienvenue sur Sqordia !','fr',1,'onboarding.welcome',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.welcome_subtitle',0,E'Configurons votre exp\u00e9rience','fr',2,'onboarding.welcome',NULL,NOW(),NOW(),false);

-- ============================================================
-- ONBOARDING - STEPS / PERSONA (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.persona_title',0,'Choose Your Profile','en',1,'onboarding.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.entrepreneur_label',0,'Entrepreneur','en',2,'onboarding.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.consultant_label',0,'Consultant','en',3,'onboarding.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.obnl_label',0,'OBNL / NPO','en',4,'onboarding.steps',NULL,NOW(),NOW(),false);

-- ONBOARDING - STEPS / PERSONA (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.persona_title',0,'Choisissez votre profil','fr',1,'onboarding.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.entrepreneur_label',0,'Entrepreneur','fr',2,'onboarding.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.consultant_label',0,'Consultant','fr',3,'onboarding.steps',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.obnl_label',0,'OBNL / OBNL','fr',4,'onboarding.steps',NULL,NOW(),NOW(),false);

-- ============================================================
-- ONBOARDING - COMPLETION (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.completion_title',0,'You''re All Set!','en',1,'onboarding.completion',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.go_to_dashboard',0,'Go to Dashboard','en',2,'onboarding.completion',NULL,NOW(),NOW(),false);

-- ONBOARDING - COMPLETION (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.completion_title',0,E'Vous \u00eates pr\u00eat !','fr',1,'onboarding.completion',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','onboarding.go_to_dashboard',0,'Aller au tableau de bord','fr',2,'onboarding.completion',NULL,NOW(),NOW(),false);

-- ============================================================
-- LEGAL - TERMS (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.terms_title',0,'Terms of Service','en',1,'legal.terms',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.terms_last_updated',0,'Last updated: January 2024','en',2,'legal.terms',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.back_to_home',0,'Back to Home','en',3,'legal.terms',NULL,NOW(),NOW(),false);

-- LEGAL - TERMS (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.terms_title',0,E'Conditions d''utilisation','fr',1,'legal.terms',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.terms_last_updated',0,E'Derni\u00e8re mise \u00e0 jour : janvier 2024','fr',2,'legal.terms',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.back_to_home',0,E'Retour \u00e0 l''accueil','fr',3,'legal.terms',NULL,NOW(),NOW(),false);

-- ============================================================
-- LEGAL - PRIVACY (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.privacy_title',0,'Privacy Policy','en',1,'legal.privacy',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.privacy_last_updated',0,'Last updated: January 2024','en',2,'legal.privacy',NULL,NOW(),NOW(),false);

-- LEGAL - PRIVACY (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.privacy_title',0,E'Politique de confidentialit\u00e9','fr',1,'legal.privacy',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','legal.privacy_last_updated',0,E'Derni\u00e8re mise \u00e0 jour : janvier 2024','fr',2,'legal.privacy',NULL,NOW(),NOW(),false);

-- ============================================================
-- GLOBAL - NAVIGATION / SIDEBAR (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.dashboard',0,'Dashboard','en',1,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.my_plans',0,'My Plans','en',2,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.create_plan',0,'Create Plan','en',3,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.profile',0,'Profile','en',4,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.settings',0,'Settings','en',5,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.help',0,'Help & Support','en',6,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.admin_cms',0,'CMS Management','en',7,'global.navigation',NULL,NOW(),NOW(),false);

-- GLOBAL - NAVIGATION / SIDEBAR (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.dashboard',0,'Tableau de bord','fr',1,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.my_plans',0,'Mes plans','fr',2,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.create_plan',0,E'Cr\u00e9er un plan','fr',3,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.profile',0,'Profil','fr',4,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.settings',0,E'Param\u00e8tres','fr',5,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.help',0,'Aide et support','fr',6,'global.navigation',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','sidebar.admin_cms',0,'Gestion CMS','fr',7,'global.navigation',NULL,NOW(),NOW(),false);
