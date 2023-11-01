CREATE SCHEMA [TRADERS]
GO

DROP TABLE IF EXISTS [TRADERS].TradingAccount;

CREATE TABLE [TRADERS].TradingAccount (
    symbol varchar(14) NOT NULL, -- 14 bytes
    time bigint NOT NULL, -- 8 bytes
    --[open] float NOT NULL, -- 8 bytes
    --high float NOT NULL, -- 8 bytes
    --low float NOT NULL, -- 8 bytes
    --[close] float NOT NULL, -- 8 bytes

    funding_rate decimal(9, 8) NOT NULL, -- 5 bytes
    --open_interest_base float NOT NULL, -- 8 bytes
    --open_interest_quote float NOT NULL, -- 8 bytes

    --global_ratio_long_short float NOT NULL, -- 8 bytes
    --total_open_interest_base float NOT NULL, -- 8 bytes
    --total_open_interest_quote float NOT NULL, -- 8 bytes

    CONSTRAINT pk_unique PRIMARY KEY (symbol, time ASC),
)

