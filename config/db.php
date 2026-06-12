<?php
$conn = mysqli_connect("localhost","root","","mosafir_db",3308);

if(!$conn){
    die("Connection failed: " . mysqli_connect_error());
}

mysqli_set_charset($conn, "utf8mb4");
?>