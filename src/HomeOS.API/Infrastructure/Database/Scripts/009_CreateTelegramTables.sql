-- Links vinculados: associa um chat_id do Telegram a um HouseholdId
CREATE TABLE TelegramLinks (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    HouseholdId UUID NOT NULL,
    ChatId BIGINT NOT NULL UNIQUE,
    LinkedByUserId UUID NOT NULL,
    LinkedAt TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Códigos temporários de vinculação (expiram em 10 minutos)
CREATE TABLE TelegramPendingLinks (
    Code VARCHAR(10) PRIMARY KEY,
    HouseholdId UUID NOT NULL,
    CreatedByUserId UUID NOT NULL,
    ExpiresAt TIMESTAMP NOT NULL
);

-- Sessão de contexto: guarda qual lista o usuário está "operando" no Telegram
CREATE TABLE TelegramSessions (
    ChatId BIGINT PRIMARY KEY,
    ActiveShoppingListId UUID NULL,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW()
);
