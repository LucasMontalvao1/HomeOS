CREATE TABLE ShoppingLists (
    Id UUID PRIMARY KEY,
    HouseholdId UUID NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Status VARCHAR(50) NOT NULL, -- Pending, InProgress, Completed, Cancelled
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ShoppingLists_Household FOREIGN KEY (HouseholdId) REFERENCES Households(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ShoppingLists_HouseholdId ON ShoppingLists(HouseholdId);
CREATE INDEX IX_ShoppingLists_Status ON ShoppingLists(Status);

CREATE TABLE ShoppingListItems (
    Id UUID PRIMARY KEY,
    ShoppingListId UUID NOT NULL,
    ProductId UUID NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL DEFAULT 1,
    Unit VARCHAR(50),
    Checked BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ShoppingListItems_ShoppingList FOREIGN KEY (ShoppingListId) REFERENCES ShoppingLists(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ShoppingListItems_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ShoppingListItems_ShoppingListId ON ShoppingListItems(ShoppingListId);
CREATE INDEX IX_ShoppingListItems_ProductId ON ShoppingListItems(ProductId);
