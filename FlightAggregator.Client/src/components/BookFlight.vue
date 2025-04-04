<template>
    <div>
        <h1>Book Flight</h1>
        <button @click="bookFlight">Confirm Booking</button>
    </div>
</template>

<script>
import axios from 'axios';
export default {
    props: ['flightId'],
    methods: {
        async bookFlight() {
            const response = await axios.post('http://localhost:5000/api/aggregator/book-flight', {
                flightId: this.flightId
            });
            alert(response.data.message || 'Booking successful!');
        }
    }
};
</script>