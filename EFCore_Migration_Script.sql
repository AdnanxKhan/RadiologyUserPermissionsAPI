CREATE TABLE UserCategories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE UserPermissions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    UserCategoryId INT NOT NULL,
    FOREIGN KEY (UserCategoryId) REFERENCES UserCategories(Id)
);