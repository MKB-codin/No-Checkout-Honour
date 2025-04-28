# No-Checkout-Honour
A self-service mobile checkout system that allows users to scan items, pay via Stripe, and receive digital receipts — bypassing physical checkout lines.


## Software Prerequisites
- [Visual Studio 2022] (https://visualstudio.microsoft.com/)
	- With Mobile development workload (MAUI)
- [.NET 9.0 SDK] (https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads))


## Setup Instructions

### 1. Database

- Open SQL Server Management Studio
- Connect to your SQL Server instance
- Create a new database named `NoCheckoutShopDb`
- Run the provided scripts in `Generate NoCheckoutShopDb` folder.
- First the `Create Tables.sql` script to create the tables.
- Then `Populate Tables.sql` to populate the tables with test data.

## Testing the Server
- Open the `NoCheckoutServer` project in Visual Studio.
- Run in Release and in `https`.
- Navigate to the ngrok folder
- Run ngrok.exe
- Run the command provided in `Command to run.txt`
- Navigate to `https://suitable-turkey-main.ngrok-free.app/index.html` to test endpoints.

## Using the App
- Ensure the server is running and ngrok is active - using the `Testing Server` steps.
- Open the `NoCheckoutApp` project in Visual Studio.
- Run in release mode to Pixel 7 API 35 Android emulator OR grab the apk `MKB.MKB-Signed.apk` from /bin/release/net9.0-android35.0 and install on your device.




### Dev Notes
Contact me at: mkbofficial16@gmail.com for any questions or issues.

There are no users in the generated database, so you will need to create a new user in the app. 

There is also a bug that sometimes happens when using the emulated android device. It thinks the mouse inputs are a stylus and thus the on-screen keyboard does not appear. 
This can be fixed by going to the settings and disabling the stylus input once in the phone.



The items are not the same in the video demonstration, as there might be an issue with different barcodes in different regions. I.e the barcode for milk in Birmingham might be different to the barcode for milk in Hull.
So some test items are given below along with how to generate the barcodes for them.

The barcodes for items are set to:
`1111111111` = Milk
`2222222222` = Bread
`3333333333` = Eggs
`4444444444` = Butter
`5555555555` = Cheese
`6666666666` = Chicken
`7777777777` = Fish

This is done for testing purposes, in reality these would be changed to the actual barcodes on the physical items to work as seen in the demonstration video.
This website can be used to generate barcodes for testing:
https://barcode.tec-it.com/en/

One final note: Remember the barcode scanner is a bit weird and for it to scan. The barcode itself must be verticle like a stack and not horizontal OR the phone must be put into landscape mode.