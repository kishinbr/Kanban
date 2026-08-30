CREATE TABLE usuarios (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    email VARCHAR(150) UNIQUE NOT NULL,
    senha_hash VARCHAR(255) NOT NULL,
    criado_em TIMESTAMP DEFAULT NOW()
);

-- Tabela de quadros (kanbans)
CREATE TABLE quadros (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    usuario_dono_id INT NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    codigo_compartilhamento VARCHAR(10) UNIQUE NOT NULL
);

-- Tabela de membros (quem participa de cada quadro, como dono ou espectador)
CREATE TABLE membros (
    id SERIAL PRIMARY KEY,
    quadro_id INT NOT NULL REFERENCES quadros(id) ON DELETE CASCADE,
    usuario_id INT NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    papel VARCHAR(20) NOT NULL DEFAULT 'espectador', -- 'dono' ou 'espectador'
    UNIQUE (quadro_id, usuario_id)
);

-- Tabela de colunas (dentro de cada quadro)
CREATE TABLE colunas (
    id SERIAL PRIMARY KEY,
    quadro_id INT NOT NULL REFERENCES quadros(id) ON DELETE CASCADE,
    nome VARCHAR(100) NOT NULL
);

-- Tabela de cartões (dentro de cada coluna)
CREATE TABLE cartoes (
    id SERIAL PRIMARY KEY,
    coluna_id INT NOT NULL REFERENCES colunas(id) ON DELETE CASCADE,
    titulo VARCHAR(200) NOT NULL,
    descricao TEXT,
    ordem INT NOT NULL DEFAULT 0
);