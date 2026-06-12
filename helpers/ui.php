<?php
// Shared UI helper functions

if (!function_exists('stars')) {
    /**
     * Renders a rating as visual stars.
     * @param float $rating
     * @return string
     */
    function stars($rating) {
        $full = floor($rating);
        $stars = str_repeat('⭐', $full);
        if ($rating - $full >= 0.5) {
            $stars .= '⭐';
        }
        return $stars . str_repeat('☆', max(0, 5 - ceil($rating)));
    }
}
?>
