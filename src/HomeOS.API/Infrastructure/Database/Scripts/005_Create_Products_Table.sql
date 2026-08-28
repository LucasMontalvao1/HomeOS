CREATE TABLE Products (
    Id UUID PRIMARY KEY,
    HouseholdId UUID NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Brand VARCHAR(100),
    Category VARCHAR(100),
    Unit VARCHAR(50),
    Barcode VARCHAR(50),
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Products_Household FOREIGN KEY (HouseholdId) REFERENCES Households(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Products_HouseholdId ON Products(HouseholdId);
CREATE INDEX IX_Products_Barcode ON Products(HouseholdId, Barcode) WHERE Barcode IS NOT NULL;
CREATE INDEX IX_Products_Name ON Products(HouseholdId, Name);
