CREATE OR REPLACE FUNCTION create_user(
    p_login TEXT,
    p_password TEXT,
    p_name TEXT,
    p_surname TEXT,
    p_age INT
) RETURNS SETOF users AS $$
BEGIN
    RETURN QUERY
    INSERT INTO users(login, password, name, surname, age)
    VALUES (p_login, p_password, p_name, p_surname, p_age)
    RETURNING *;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_user_by_id(p_id INT)
RETURNS SETOF users AS $$
BEGIN
    RETURN QUERY SELECT * FROM users WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_user_by_name(p_name TEXT, p_surname TEXT)
RETURNS SETOF users AS $$
BEGIN
    RETURN QUERY SELECT * FROM users WHERE name = p_name AND surname = p_surname;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_user(
    p_id INT,
    p_password TEXT,
    p_name TEXT,
    p_surname TEXT,
    p_age INT
) RETURNS SETOF users AS $$
BEGIN
    RETURN QUERY
    UPDATE users
    SET password = p_password,
        name = p_name,
        surname = p_surname,
        age = p_age
    WHERE id = p_id
    RETURNING *;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_user(p_id INT)
RETURNS BOOLEAN AS $$
BEGIN
    DELETE FROM users WHERE id = p_id;
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;