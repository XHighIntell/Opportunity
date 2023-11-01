CREATE SCHEMA MARKET
GO

DROP TABLE IF EXISTS [MARKET].Historical;

CREATE TABLE [MARKET].Historical (
    symbol varchar(14) NOT NULL, -- 14 bytes
    time bigint NOT NULL, -- 8 bytes
    funding_rate decimal(9, 8) NOT NULL, -- 5 bytes
    CONSTRAINT pk_unique PRIMARY KEY (symbol, time ASC),
)


INSERT INTO [MARKET].Historical(symbol, time, funding_rate) VALUES('XMRUSDT', 1, 0.000132),('XMRUSDT', 2, 0.000132)
INSERT INTO [MARKET].Historical(symbol, time, funding_rate) VALUES('XMRUSDT', 3, 0.000132),('XMRUSDT', 4, 0.000134)
INSERT INTO [MARKET].Historical(symbol, time, funding_rate) VALUES('BTCUSDT', 3, 0.000132)

SELECT * FROM [MARKET].Historical

DECLARE @MyCounter bigint = (SELECT TOP 1 time FROM [MARKET].Historical WHERE symbol='XMRUSDT' ORDER BY time desc);
SELECT @MyCounter

SELECT TOP 1 * FROM [MARKET].Historical WHERE symbol='XMRUSDT' ORDER BY time desc

DECLARE @previous_funding_rate decimal(9, 8)
SET @previous_funding_rate = 2;
SELECT @previous_funding_rate;

GO;

CREATE TRIGGER trg_trigger_name ON [MARKET].Historical INSTEAD OF INSERT 
AS
BEGIN
	DECLARE @symbol varchar(14);
	DECLARE @time bigint;
	DECLARE @funding_rate decimal(9, 8);
	DECLARE @previous_funding_rate decimal(9, 8);

	DECLARE insert_cursor CURSOR LOCAL FOR SELECT * FROM inserted;

	OPEN insert_cursor; 

	FETCH NEXT FROM insert_cursor INTO @symbol, @time, @funding_rate;  

	WHILE @@FETCH_STATUS = 0  
	BEGIN  
	   -- PRINT 'Contact Name: ' + @symbol + ' ' +  @time + ' ' + @funding_rate
		
		SET @previous_funding_rate = (SELECT TOP 1 funding_rate FROM [MARKET].Historical WHERE symbol=@symbol ORDER BY time desc);

		IF (@funding_rate <> @previous_funding_rate) 
			INSERT INTO [MARKET].Historical SELECT * FROM inserted;

	   -- This is executed as long as the previous fetch succeeds.  
	   FETCH NEXT FROM insert_cursor INTO @symbol, @time, @funding_rate;  
	END  

	CLOSE insert_cursor;  
	DEALLOCATE insert_cursor; 
END;