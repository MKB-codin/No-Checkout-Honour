-- INSERT STORES
INSERT INTO Stores (StoreName, Location)
VALUES 
('Tesco Local', '221B Baker Street, London'),
('Sainsbury Central', '10 Downing Street, London'),
('Co-op City', '1 Piccadilly, Manchester');

-- INSERT PRODUCTS
INSERT INTO Products (ProductName, BarcodeId)
VALUES 
('Milk', '1111111111'),
('Bread', '2222222222'),
('Eggs', '3333333333'),
('Butter', '4444444444'),
('Cheese', '5555555555'),
('Chicken', '6666666666'),
('Fish', '7777777777');

-- LINK PRODUCTS TO EACH STORE WITH PRICES

-- StoreId 1 (Tesco Local)
INSERT INTO StoreProduct (StoreId, ProductId, Price)
VALUES 
(1, 1, 1.29), -- Milk
(1, 2, 0.99), -- Bread
(1, 3, 2.49), -- Eggs
(1, 4, 1.75), -- Butter
(1, 5, 2.99), -- Cheese
(1, 6, 4.50), -- Chicken
(1, 7, 5.99); -- Fish

-- StoreId 2 (Sainsbury Central)
INSERT INTO StoreProduct (StoreId, ProductId, Price)
VALUES 
(2, 1, 1.35),
(2, 2, 1.05),
(2, 3, 2.39),
(2, 4, 1.80),
(2, 5, 3.10),
(2, 6, 4.75),
(2, 7, 6.10);

-- StoreId 3 (Co-op City)
INSERT INTO StoreProduct (StoreId, ProductId, Price)
VALUES 
(3, 1, 1.20),
(3, 2, 1.00),
(3, 3, 2.50),
(3, 4, 1.70),
(3, 5, 2.95),
(3, 6, 4.60),
(3, 7, 5.85);
