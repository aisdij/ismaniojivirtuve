export function geneticAlgorithm(waypoints, origin, destination) {
    const populationSize = 20;
    const generations = 50;
    const mutationRate = 0.1;

    if (waypoints.length < 2) {
        console.error("Not enough waypoints to perform genetic algorithm.");
        return waypoints;
    }


    let population = createInitialPopulation(waypoints.slice(0, -1), populationSize);

    for (let gen = 0; gen < generations; gen++) {

        let fitness = population.map(route => calculateFitness(route, origin, destination));

        let selected = selection(population, fitness);

        let offspring = [];
        for (let i = 0; i < selected.length / 2; i++) {
            let parents = selected.slice(i * 2, (i + 1) * 2);
            if (parents.length < 2) {
                console.error("Not enough parents selected for crossover:", parents);
                continue;
            }
            offspring.push(...crossover(parents[0], parents[1]));
        }
        population = offspring.map(route => mutate(route, mutationRate));
    }
    let bestRoute = population.reduce((best, route) => {
        return calculateFitness(route, origin, destination) < calculateFitness(best, origin, destination) ? route : best;
    });
    bestRoute.push(waypoints[waypoints.length - 1]);

    return bestRoute;
}

function createInitialPopulation(waypoints, size) {
    let population = [];
    for (let i = 0; i < size; i++) {
        population.push(shuffleArray(waypoints.slice()));
    }
    return population;
}

function shuffleArray(array) {
    for (let i = array.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [array[i], array[j]] = [array[j], array[i]];
    }
    return array;
}

function calculateFitness(route, origin, destination) {
    let totalDistance = 0;
    let lastPoint = new google.maps.LatLng(origin.lat, origin.lng);
    for (let waypoint of route) {
        let waypointLocation = new google.maps.LatLng(waypoint.location.lat, waypoint.location.lng);
        totalDistance += google.maps.geometry.spherical.computeDistanceBetween(lastPoint, waypointLocation);
        lastPoint = waypointLocation;
    }
    let destinationLocation = new google.maps.LatLng(destination.lat, destination.lng);
    totalDistance += google.maps.geometry.spherical.computeDistanceBetween(lastPoint, destinationLocation);
    return totalDistance;
}

function selection(population, fitness) {
    let selected = [];
    let totalFitness = fitness.reduce((a, b) => a + b, 0);
    let probabilities = fitness.map(fit => fit / totalFitness);

    let cumulativeProbabilities = [];
    probabilities.reduce((acc, prob, index) => {
        acc += prob;
        cumulativeProbabilities[index] = acc;
        return acc;
    }, 0);

    for (let i = 0; i < population.length; i++) {
        let r = Math.random();
        let index = binarySearch(cumulativeProbabilities, r);
        selected.push(population[index]);
    }

    return selected;
}

function binarySearch(arr, target) {
    let left = 0;
    let right = arr.length - 1;
    while (left < right) {
        let mid = Math.floor((left + right) / 2);
        if (arr[mid] < target) {
            left = mid + 1;
        } else {
            right = mid;
        }
    }
    return left;
}

function crossover(parent1, parent2) {
    const crossoverPoint = Math.floor(Math.random() * parent1.length);
    const child1 = [...new Set(parent1.slice(0, crossoverPoint).concat(parent2.slice(crossoverPoint)))];
    const child2 = [...new Set(parent2.slice(0, crossoverPoint).concat(parent1.slice(crossoverPoint)))];
    return [ensureAllWaypoints(child1, parent1), ensureAllWaypoints(child2, parent2)];
}

function ensureAllWaypoints(child, parent) {
    let childSet = new Set(child.map(wp => JSON.stringify(wp)));
    parent.forEach(wp => {
        if (!childSet.has(JSON.stringify(wp))) {
            child.push(wp);
        }
    });
    return child;
}

function mutate(route, mutationRate) {
    if (Math.random() < mutationRate) {
        const index1 = Math.floor(Math.random() * route.length);
        const index2 = (index1 + 1) % route.length;
        [route[index1], route[index2]] = [route[index2], route[index1]];
    }
    return route;
}

export function displayRoute(route, origin, destination, directionsService, directionsRenderer, travelMode) {
    const alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const waypoints = route.map((wp, index) => ({
        location: wp.location,
        stopover: true
    }));
    const directionsRequest = {
        origin: origin,
        destination: destination,
        waypoints: waypoints,
        travelMode: travelMode
    };

    directionsService.route(directionsRequest, function (response, status) {
        if (status === google.maps.DirectionsStatus.OK) {
            directionsRenderer.setDirections(response);
            for (let i = 0; i < waypoints.length; i++) {
                const marker = new google.maps.Marker({
                    position: waypoints[i].location,
                    label: alphabet[i],
                    map: map
                });
            }
        } else {
            window.alert('Directions request failed due to ' + status);
        }
    });
}