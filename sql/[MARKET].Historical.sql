CREATE SCHEMA MARKET
GO

DROP TABLE IF EXISTS [MARKET].Historical;

CREATE TABLE [MARKET].Historical (
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


INSERT INTO [MARKET].Historical(symbol, time, funding_rate) 
VALUES('XMRUSDT', GETUTCDATE(), 0)

INSERT INTO [MARKET].Historical(symbol, time, [open], high, low, [close], 
    funding_rate, open_interest_base , global_ratio_long_short, total_open_interest_base, total_open_interest_quote) 

VALUES('XMRUSDT', GETUTCDATE(), 0, 0, 0, 0,
    0.0001, 0, 0, 0, 0, 0)