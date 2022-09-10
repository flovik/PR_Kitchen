# Network Programming - Laboratory work nr. 1 - Kitchen server
## The purpose of the laboratory work
The purpose of the laboratory work is to create two APIs that simulate a restaurant workflow. Dining Hall server communicates with Kitchen server and vice versa. 

Dining hall generates *orders* and gives these orders to the **Kitchen** which prepares them and returns prepared orders back to the Dining hall. 

## Link to Dining hall server
[Dining hall](https://github.com/flovik/PR_Dining-Hall)

## Run app with Docker
docker build -t kitchen .

docker run -p 8081:8081 kitchen
