-- =============================================================================
-- MatchIQ — Fix: alinear strings SQL con PascalCase de EF Core
-- Ejecutar en DBeaver con Alt+X (Execute SQL Script), NO con Ctrl+Enter.
-- Envuelto en transacción: si algo falla, hace rollback completo.
-- =============================================================================

BEGIN;

-- =============================================================================
-- PASO 1: NORMALIZAR DATOS EXISTENTES
-- Los enums viejos de PostgreSQL guardaban snake_case/minúscula.
-- EF Core HasConversion<string>() guarda PascalCase.
-- Este bloque convierte los valores viejos al formato correcto.
-- =============================================================================

-- users.role (el rename de enum ya lo dejó PascalCase, pero por seguridad)
UPDATE users SET role = 'Admin'     WHERE role = 'admin';
UPDATE users SET role = 'Candidate' WHERE role = 'candidate';
UPDATE users SET role = 'Company'   WHERE role = 'company';

-- candidate_profiles.seniority
UPDATE candidate_profiles SET seniority = 'Junior' WHERE seniority = 'junior';
UPDATE candidate_profiles SET seniority = 'Mid'    WHERE seniority = 'mid';
UPDATE candidate_profiles SET seniority = 'Senior' WHERE seniority = 'senior';

-- job_offers.modality
UPDATE job_offers SET modality = 'Remote' WHERE modality = 'remote';
UPDATE job_offers SET modality = 'Onsite' WHERE modality = 'onsite';
UPDATE job_offers SET modality = 'Hybrid' WHERE modality = 'hybrid';

-- job_offers.status
UPDATE job_offers SET status = 'PendingPayment' WHERE status = 'pending_payment';
UPDATE job_offers SET status = 'Open'           WHERE status = 'open';
UPDATE job_offers SET status = 'TestSent'       WHERE status = 'test_sent';
UPDATE job_offers SET status = 'Completed'      WHERE status = 'completed';
UPDATE job_offers SET status = 'Cancelled'      WHERE status = 'cancelled';
UPDATE job_offers SET status = 'Expired'        WHERE status = 'expired';

-- payments.status
UPDATE payments SET status = 'Pending'   WHERE status = 'pending';
UPDATE payments SET status = 'Succeeded' WHERE status = 'succeeded';
UPDATE payments SET status = 'Failed'    WHERE status = 'failed';
UPDATE payments SET status = 'Refunded'  WHERE status = 'refunded';

-- matches.stage
UPDATE matches SET stage = 'Matched'       WHERE stage = 'matched';
UPDATE matches SET stage = 'TestSent'      WHERE stage = 'test_sent';
UPDATE matches SET stage = 'TestCompleted' WHERE stage = 'test_completed';
UPDATE matches SET stage = 'Selected'      WHERE stage = 'selected';
UPDATE matches SET stage = 'Rejected'      WHERE stage = 'rejected';

-- test_questions.question_type
UPDATE test_questions SET question_type = 'MultipleChoice' WHERE question_type = 'multiple_choice';
UPDATE test_questions SET question_type = 'CodeChallenge'  WHERE question_type = 'code_challenge';

-- question_chat_messages.role
UPDATE question_chat_messages SET role = 'Admin'     WHERE role = 'admin';
UPDATE question_chat_messages SET role = 'Assistant' WHERE role = 'assistant';

-- test_submissions.status
UPDATE test_submissions SET status = 'Pending'   WHERE status = 'pending';
UPDATE test_submissions SET status = 'Evaluated' WHERE status = 'evaluated';
UPDATE test_submissions SET status = 'Expired'   WHERE status = 'expired';


-- =============================================================================
-- PASO 2: RECREAR ÍNDICES PARCIALES
-- Filtraban por los valores en minúscula/snake_case — deben coincidir con datos.
-- =============================================================================

DROP INDEX IF EXISTS idx_offers_test_sent_status;
CREATE INDEX idx_offers_test_sent_status ON job_offers(status) WHERE status = 'TestSent';

DROP INDEX IF EXISTS idx_submissions_deadline;
CREATE INDEX idx_submissions_deadline ON test_submissions(deadline) WHERE status = 'Pending';


-- =============================================================================
-- PASO 3: DROPEAR EL TRIGGER ANTES DE LA FUNCIÓN (dependencia)
-- =============================================================================

DROP TRIGGER IF EXISTS trg_candidate_profile_rematch ON candidate_profiles;


-- =============================================================================
-- PASO 4: RECREAR FUNCIONES CON VALORES PASCALCASE
-- =============================================================================

-- ----------------------------------------------------------------------------
-- get_full_offer_ranking: inserta 'Matched' (antes 'matched')
-- ----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION get_full_offer_ranking(p_offer_id INTEGER)
RETURNS TABLE (
    candidate_id        INTEGER,
    match_percentage    NUMERIC,
    was_already_matched BOOLEAN
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_min_exp      INTEGER;
    v_req_english  VARCHAR(10);
    v_total_skills INTEGER;
    v_total_cats   INTEGER;
    english_order  TEXT[] := ARRAY['A1','A2','B1','B2','C1','C2'];
BEGIN
    SELECT min_experience_years, required_english_level
    INTO   v_min_exp, v_req_english
    FROM   job_offers
    WHERE  id = p_offer_id;

    SELECT COUNT(*) INTO v_total_skills
    FROM offer_skills WHERE offer_id = p_offer_id;

    SELECT COUNT(*) INTO v_total_cats
    FROM offer_categories WHERE offer_id = p_offer_id;

    RETURN QUERY
    WITH scored AS (
        SELECT
            cp.id AS cand_id,
            ROUND(
                CASE WHEN v_total_skills = 0 THEN 50
                     ELSE (COUNT(DISTINCT cs.skill_id)::NUMERIC / v_total_skills) * 50 END
                +
                CASE WHEN v_total_cats = 0 THEN 20
                     ELSE (COUNT(DISTINCT cc.category_id)::NUMERIC / v_total_cats) * 20 END
                +
                CASE WHEN v_min_exp IS NULL THEN 20
                     WHEN cp.experience_years >= v_min_exp THEN 20
                     ELSE (cp.experience_years::NUMERIC / NULLIF(v_min_exp, 0)) * 20 END
                +
                CASE WHEN v_req_english IS NULL THEN 10
                     WHEN cp.english_level IS NULL THEN 0
                     WHEN ARRAY_POSITION(english_order, cp.english_level)
                        >= ARRAY_POSITION(english_order, v_req_english) THEN 10
                     ELSE (ARRAY_POSITION(english_order, cp.english_level)::NUMERIC
                        / ARRAY_POSITION(english_order, v_req_english)) * 10 END
            , 2) AS score
        FROM candidate_profiles cp
        LEFT JOIN candidate_skills cs
               ON cs.candidate_id = cp.id
              AND cs.skill_id IN (SELECT skill_id FROM offer_skills WHERE offer_id = p_offer_id)
        LEFT JOIN candidate_categories cc
               ON cc.candidate_id = cp.id
              AND cc.category_id IN (SELECT category_id FROM offer_categories WHERE offer_id = p_offer_id)
        JOIN users u ON u.id = cp.user_id AND u.is_active = TRUE
        GROUP BY cp.id, cp.experience_years, cp.english_level
    ),
    upserted AS (
        INSERT INTO matches (offer_id, candidate_id, match_percentage, stage)
        SELECT p_offer_id, scored.cand_id, scored.score, 'Matched'
        FROM scored
        ON CONFLICT (offer_id, candidate_id)
        DO UPDATE SET
            match_percentage = EXCLUDED.match_percentage,
            updated_at = CURRENT_TIMESTAMP
        RETURNING candidate_id, match_percentage, (xmax != 0) AS was_update
    )
    SELECT
        upserted.candidate_id,
        upserted.match_percentage,
        upserted.was_update AS was_already_matched
    FROM upserted
    ORDER BY upserted.match_percentage DESC;
END;
$$;


-- ----------------------------------------------------------------------------
-- trigger_rematch_open_offers: compara 'Open', inserta 'Matched'
-- ----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION trigger_rematch_open_offers()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_offer RECORD;
BEGIN
    FOR v_offer IN
        SELECT id FROM job_offers
        WHERE status = 'Open'
          AND (expires_at IS NULL OR expires_at > CURRENT_TIMESTAMP)
    LOOP
        INSERT INTO matches (offer_id, candidate_id, match_percentage, stage)
        SELECT v_offer.id, gcm.candidate_id, gcm.final_match_percentage, 'Matched'
        FROM get_candidate_matches(v_offer.id) gcm
        WHERE gcm.candidate_id = NEW.id
        ON CONFLICT (offer_id, candidate_id) DO NOTHING;
    END LOOP;

    RETURN NEW;
END;
$$;


-- ----------------------------------------------------------------------------
-- send_test_to_candidates: compara 'Open', set 'TestSent' y 'Pending'
-- ----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION send_test_to_candidates(
    p_offer_id      INTEGER,
    p_candidate_ids INTEGER[]
)
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_offer_status   VARCHAR(50);
    v_test_id        INTEGER;
    v_time_limit_min INTEGER;
    v_sent_count     INTEGER;
BEGIN
    SELECT status INTO v_offer_status
    FROM job_offers WHERE id = p_offer_id;

    IF v_offer_status IS NULL THEN
        RAISE EXCEPTION 'La oferta % no existe', p_offer_id;
    END IF;

    IF v_offer_status != 'Open' THEN
        RAISE EXCEPTION 'La oferta % no está abierta (estado actual: %), no se puede enviar el test',
            p_offer_id, v_offer_status;
    END IF;

    SELECT id, time_limit_minutes INTO v_test_id, v_time_limit_min
    FROM tests WHERE offer_id = p_offer_id;

    IF v_test_id IS NULL THEN
        RAISE EXCEPTION 'La oferta % no tiene un test generado todavía', p_offer_id;
    END IF;

    UPDATE matches
    SET stage = 'TestSent', updated_at = CURRENT_TIMESTAMP
    WHERE offer_id = p_offer_id
      AND candidate_id = ANY(p_candidate_ids);

    INSERT INTO test_submissions (test_id, candidate_id, status, started_at, deadline)
    SELECT
        v_test_id,
        cid,
        'Pending',
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP + (v_time_limit_min || ' minutes')::INTERVAL
    FROM UNNEST(p_candidate_ids) AS cid
    ON CONFLICT (test_id, candidate_id) DO NOTHING;

    GET DIAGNOSTICS v_sent_count = ROW_COUNT;

    UPDATE job_offers
    SET status                  = 'TestSent',
        candidates_to_test      = v_sent_count,
        candidates_tested_count = v_sent_count,
        test_sent_at            = CURRENT_TIMESTAMP
    WHERE id = p_offer_id;

    RETURN v_sent_count;
END;
$$;


-- ----------------------------------------------------------------------------
-- expire_stale_submissions: compara 'Pending', set 'Expired'
-- ----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION expire_stale_submissions()
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_expired_count INTEGER;
BEGIN
    UPDATE test_submissions
    SET status = 'Expired'
    WHERE status = 'Pending'
      AND deadline IS NOT NULL
      AND deadline <= CURRENT_TIMESTAMP;

    GET DIAGNOSTICS v_expired_count = ROW_COUNT;
    RETURN v_expired_count;
END;
$$;


-- ----------------------------------------------------------------------------
-- expire_stale_offers: compara 'Open', set 'Expired'
-- ----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION expire_stale_offers()
RETURNS INTEGER
LANGUAGE plpgsql
AS $$
DECLARE
    v_expired_count INTEGER;
BEGIN
    UPDATE job_offers
    SET status = 'Expired'
    WHERE status = 'Open'
      AND expires_at IS NOT NULL
      AND expires_at <= CURRENT_TIMESTAMP;

    GET DIAGNOSTICS v_expired_count = ROW_COUNT;
    RETURN v_expired_count;
END;
$$;


-- ----------------------------------------------------------------------------
-- get_offer_ranking_for_company: compara 'Selected'
-- ----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION get_offer_ranking_for_company(p_offer_id INTEGER)
RETURNS TABLE (
    match_id          INTEGER,
    candidate_id      INTEGER,
    match_percentage  NUMERIC,
    stage             VARCHAR(50),
    profile_photo_url TEXT,
    seniority         VARCHAR(50),
    experience_years  INTEGER,
    english_level     VARCHAR(10),
    github_link       TEXT,
    email             TEXT,
    is_unlocked       BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        m.id AS match_id,
        cp.id AS candidate_id,
        m.match_percentage,
        m.stage,
        cp.profile_photo_url,
        cp.seniority,
        cp.experience_years,
        cp.english_level,
        cp.github_link,
        CASE WHEN m.stage = 'Selected' THEN u.email ELSE NULL END AS email,
        (m.stage = 'Selected') AS is_unlocked
    FROM matches m
    JOIN candidate_profiles cp ON cp.id = m.candidate_id
    JOIN users u ON u.id = cp.user_id
    WHERE m.offer_id = p_offer_id
    ORDER BY m.match_percentage DESC;
END;
$$;


-- =============================================================================
-- PASO 5: RECREAR TRIGGER DE MATCHING
-- =============================================================================

CREATE TRIGGER trg_candidate_profile_rematch
AFTER INSERT OR UPDATE ON candidate_profiles
FOR EACH ROW EXECUTE FUNCTION trigger_rematch_open_offers();


-- =============================================================================
-- VERIFICACIÓN (opcional — puedes ejecutar estas líneas por separado después)
-- =============================================================================
-- SELECT DISTINCT status FROM job_offers;
-- SELECT DISTINCT stage  FROM matches;
-- SELECT DISTINCT status FROM test_submissions;
-- SELECT DISTINCT status FROM payments;
-- SELECT DISTINCT question_type FROM test_questions;
-- SELECT DISTINCT seniority FROM candidate_profiles;

COMMIT;
