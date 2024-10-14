# Directory structure of the server

- **"*server_name*"**
  - **db**/
    - **"*database_name*"**/
      - **records**/
        - **"*table_name*"**
          - **"*table_name*".def** ------ *File containing definitions for all columns in the table*
          - **"*table_name*".record** ------ *File containing records of raw data of the table*
      - **"*database_name*".conf** ------ *File containing a metadata of the database*
  - **dbs** ------ *File containing names of all databases in the server*
  - **"*server_name*".conf** ------ *File containing a metadata of the server*