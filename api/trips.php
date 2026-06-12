<?php
header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST');
header('Access-Control-Allow-Headers: Content-Type');

require_once "../config/db.php";

$action = $_GET['action'] ?? '';

switch ($action) {
    case 'search':
        $from_city = $_GET['from_city'] ?? '';
        $to_city = $_GET['to_city'] ?? '';
        $max_price = (float)($_GET['max_price'] ?? 0);
        $min_rating = (float)($_GET['min_rating'] ?? 0);
        
        $sql = "SELECT t.id, t.from_city, t.from_location, t.to_city, t.trip_time, t.seats, t.price_per_seat, t.description, t.vehicle_info,
                       u.name as driver_name, u.rating as driver_rating, u.vehicle_model, u.plate_number
                FROM trips t JOIN users u ON t.driver_id = u.id 
                WHERE t.from_city LIKE ? AND t.to_city LIKE ? AND t.status = 'open'";
        $params = ["%$from_city%", "%$to_city%"];
        $types = "ss";
        if ($max_price > 0) { $sql .= " AND t.price_per_seat <= ?"; $params[] = $max_price; $types .= "d"; }
        if ($min_rating > 0) { $sql .= " AND u.rating >= ?"; $params[] = $min_rating; $types .= "d"; }
        $sql .= " ORDER BY t.trip_time ASC LIMIT 50";
        
        $stmt = $conn->prepare($sql);
        $stmt->bind_param($types, ...$params);
        $stmt->execute();
        $result = $stmt->get_result();
        $trips = [];
        while ($row = $result->fetch_assoc()) $trips[] = $row;
        echo json_encode(['success' => true, 'trips' => $trips]);
        break;
        
    case 'my_trips':
        // Auth via token/header in production
        echo json_encode(['success' => true, 'message' => 'Driver trips API']);
        break;
        
    default:
        echo json_encode(['error' => 'Invalid action']);
}
?>

