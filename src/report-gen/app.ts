import express, { Router, Request, Response } from 'express';
import { makeMap } from './routes';

// Server

const app = express();
const port = process.env.PORT || 3000;

app.get('/status', (req: Request, res: Response) => {
    res.send('ok');
});

app.post('/map', makeMap);

app.listen(port, () => {
    console.log('Microservice listening on port ' + port);
});

app.use(express.json());

app.use((err: Error, req: Request, res: Response) => {
    console.error(err.stack);
    res.status(500).send({ error: 'Something went wrong!' });
});