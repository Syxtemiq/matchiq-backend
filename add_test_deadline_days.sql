-- MatchIQ — Agregar test_deadline_days a job_offers
-- Ejecutar en DBeaver con Alt+X (Execute SQL Script)

ALTER TABLE job_offers
ADD COLUMN test_deadline_days INTEGER NOT NULL DEFAULT 3;
